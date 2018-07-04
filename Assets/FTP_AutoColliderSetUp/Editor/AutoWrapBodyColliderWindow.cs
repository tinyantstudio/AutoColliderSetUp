using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class AutoWrapBodyColliderWindow : EditorWindow
    {
        [MenuItem("FTP_Tools/FTP - AutoWrapHumanBodyColliders", false, 2000)]
        public static void DoWindow()
        {
            var window = GetWindow<AutoWrapBodyColliderWindow>("FastToolsPackage.AutoWrapBodyCollider");
            window.minSize = new Vector3(750, 600);
            window.Show();
        }

        private GameObject _targetObj;
        public HumanBodyBoneReferenceData _boneReference;
        private Vector2 _scrollPos = Vector2.zero;

        private float _layOutLeftMinWidth = 350f;
        private float _layOutTopHeight = 20;
        private Rect _rectLayOutLeft;
        private Rect _rectLayOutRight;

        private Dictionary<HumanBodyBones, bool> _dicFoldStatus = new Dictionary<HumanBodyBones, bool>();
        private Dictionary<HumanBodyBones, bool> _dicLinePointStatus = new Dictionary<HumanBodyBones, bool>();
        private bool _clickedAutoCreate = false;

        private GameObject handler001;
        private GameObject handler002;
        private LineSphereCollider _currentEditHandlers = null;

        public enum AutoWrapColliderMode
        {
            Normal = 0,
            LineSphereFakeCollider,
        }

        private float _lastTime = 0.0f;
        private AutoWrapColliderMode _mode = AutoWrapColliderMode.Normal;
        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - _lastTime >= 0.333f)
            {
                _lastTime = (float)EditorApplication.timeSinceStartup;
                this.Repaint();
            }
        }

        public void ShutDownHandler()
        {
            if (handler001 != null)
                GameObject.DestroyImmediate(handler001.gameObject);
            if (handler002 != null)
                GameObject.DestroyImmediate(handler002.gameObject);
            _currentEditHandlers = null;
        }

        private void OnEnable()
        {
            EditorApplication.update += (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(OnEditorUpdate));
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            if (_boneReference == null)
            {
                _boneReference = new HumanBodyBoneReferenceData();
            }
            else
                _boneReference.ResetReference();
            RefreshTargetScaleFactor(_targetObj);
            _targetObj = null;
            _lastTime = (float)EditorApplication.timeSinceStartup;
            _clickedAutoCreate = false;
        }

        private void OnDisable()
        {
            EditorApplication.update -= (EditorApplication.CallbackFunction)System.Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(OnEditorUpdate));
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            RefreshTargetScaleFactor(_targetObj);
            ShutDownHandler();
            _linePointCS.DestroyHandlers();
        }

        private void RefreshTargetScaleFactor(GameObject root)
        {
            if (root == null)
                return;
            LineSphereCollider[] preColliders = root.GetComponentsInChildren<LineSphereCollider>(true);
            for (int i = 0; i < preColliders.Length; i++)
            {
                preColliders[i].RefreshRealFactor();
            }
        }

        private void OnGUI()
        {
            _rectLayOutLeft = new Rect(0f, _layOutTopHeight, _layOutLeftMinWidth, position.height - _layOutTopHeight);
            _rectLayOutRight = new Rect(_layOutLeftMinWidth, _layOutTopHeight, position.width - _layOutLeftMinWidth, position.height - _layOutTopHeight);
            if (EditorGUIUtility.isProSkin)
                GUILayout.BeginArea(_rectLayOutLeft, GUIContent.none, "CurveEditorBackground");
            else
                GUILayout.BeginArea(_rectLayOutLeft, GUIContent.none);
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Can't edit Collider in Playing Mode!", MessageType.Warning);
                GUILayout.EndArea();
                return;
            }

            EditorTools.DrawSpace(1);
            EditorTools.DrawLabelWithColorInBox("FastToolsPackage.AutoWrapBodyCollider Tools Window", Color.green);
            EditorTools.DrawSpace(3);

            _mode = (AutoWrapColliderMode)EditorGUILayout.EnumPopup("Wrap Collider Mode", _mode);
            GUI.changed = false;
            GameObject preTarget = _targetObj;
            _targetObj = EditorGUILayout.ObjectField("Target", _targetObj, typeof(GameObject), true) as GameObject;
            if (GUI.changed)
            {
                _clickedAutoCreate = false;
                _boneReference.ResetReference();
                RefreshTargetScaleFactor(preTarget);
                ShutDownHandler();
                // line sphere setting
                if (_targetObj != null && preTarget != _targetObj)
                {
                    LineSphereCollider lsc = _targetObj.GetComponentInChildren<LineSphereCollider>();
                    _linePointCS._initLineSphereOver = (lsc != null);
                    // Debug.Log("## change to different Target object ##");
                }
            }

            if (_targetObj == null)
            {
                EditorGUILayout.HelpBox("Please Select Target Object!", MessageType.Warning);
            }
            else
            {
                EditorTools.DrawSpace(1);
                EditorTools.DrawLabelWithColorInBox("Mapping Human Bone Reference", Color.green);
                ShowBoneReference(_boneReference);
                EditorTools.DrawSpace(1);
                _showBones = GUILayout.Toggle(_showBones, "ShowBones");
                if (GUILayout.Button("Mapping Bone With Human Avatar", GUILayout.MinHeight(50f)))
                {
                    ReMappingWithAvatar();
                }
                EditorTools.DrawSpace(2);
                if (!_clickedAutoCreate && _mode == AutoWrapColliderMode.Normal && GUILayout.Button("Auto Create Collider By Settings", GUILayout.MinHeight(50)))
                {
                    if (_boneReference.IsValid())
                    {
                        AutoCreateCollier();
                        _clickedAutoCreate = true;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Humanoid Avatar config is invalid", "OK");
                    }
                }

                if (_clickedAutoCreate && _boneReference.IsValid())
                {
                    AutoCreateCollier();
                }

                if (_mode == AutoWrapColliderMode.LineSphereFakeCollider && GUILayout.Button("Reset Line Point Collider", GUILayout.MinHeight(50)))
                {
                    _linePointCS.DestroyHandlers();
                    _linePointCS = new LineSphereColliderSettings();
                    ResetLinePointWrapCollider();
                }

                if (_mode == AutoWrapColliderMode.LineSphereFakeCollider)
                {
                    // set all settings
                    LineSphereCollider[] allCollider = _targetObj.GetComponentsInChildren<LineSphereCollider>(true);
                    for (int i = 0; i < allCollider.Length; i++)
                    {
                        allCollider[i].EnableColliderWithRootBone(HumanBodyBones.Chest, _linePointCS.Chest);
                        allCollider[i].EnableColliderWithRootBone(HumanBodyBones.Spine, _linePointCS.Spine);
                        allCollider[i].EnableColliderWithRootBone(HumanBodyBones.Hips, _linePointCS.Hip);
                        allCollider[i].EnableColliderWithRootBone(HumanBodyBones.Neck, _linePointCS.Neck);
                    }
                }
                if (SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.Repaint();
            }
            GUILayout.EndArea();
            // draw collider settings
            if (_targetObj != null)
            {
                if (EditorGUIUtility.isProSkin)
                    GUILayout.BeginArea(_rectLayOutRight, GUIContent.none, "CurveEditorBackground");
                else
                    GUILayout.BeginArea(_rectLayOutRight, GUIContent.none);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                if (_mode == AutoWrapColliderMode.Normal)
                {
                    DrawNormalModeColliderSettings();
                }
                else if (_mode == AutoWrapColliderMode.LineSphereFakeCollider)
                {
                    if (!_boneReference.IsValid())
                        EditorGUILayout.HelpBox("Config Body Collider Mapping Human Bones First!", MessageType.Error);
                    else
                        DrawLinePointModeSetting();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        private void ResetLinePointWrapCollider()
        {
            ShutDownHandler();
            if (_boneReference.IsValid())
            {
                BodyJointChainsManager.CreateJoints(_targetObj.transform, _boneReference, true);
                _linePointCS.CreateHandlers(_boneReference);
            }
            else
            {
                EditorUtility.DisplayDialog("Information",
                "Please Auto Mappint Humanoid Bones First",
                "OK");
            }
        }

        private void AutoCreateCollier()
        {
            if (_mode == AutoWrapColliderMode.Normal)
                AutoCreateColliderForHumanBody(_targetObj.transform, _boneReference, _settings);
        }

        private void DrawLinePointModeSetting()
        {
            EditorTools.DrawSpace(1);
            EditorTools.DrawLabelWithColorInBox("Wrap Line Point Body Collider Settings", Color.green);
            EditorTools.DrawSpace(1);
            _linePointCS.Spine = EditorGUILayout.Toggle("Spine", _linePointCS.Spine);
            _linePointCS.Chest = EditorGUILayout.Toggle("Chest", _linePointCS.Chest);
            _linePointCS.Hip = EditorGUILayout.Toggle("Hip", _linePointCS.Hip);
            _linePointCS.Neck = EditorGUILayout.Toggle("Neck", _linePointCS.Neck);

            // Init setting
            if (!_linePointCS._initLineSphereOver)
            {
                EditorGUILayout.HelpBox("Config Body Collider!", MessageType.Warning);
                RootColliderManager[] mgs = _targetObj.GetComponentsInChildren<RootColliderManager>();
                foreach (var setting in _linePointCS._settings)
                {
                    LineSphereColliderSubSetting set = setting.Value;
                    HumanBodyBones bone = setting.Key;
                    string partName = GetBodyBoneName(bone);
                    set.factor = EditorGUILayout.Slider(partName + " Factor", set.factor, 0.01f, 5f);
                    set.segment = EditorGUILayout.IntSlider(partName + " Segment", set.segment, 1, 6);

                    EditorTools.DrawSpace(2);
                    for (int i = 0; i < mgs.Length; i++)
                    {
                        if (bone == HumanBodyBones.LeftShoulder)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightShoulder, set.factor);
                        else if (bone == HumanBodyBones.LeftUpperArm)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightUpperArm, set.factor);
                        else if (bone == HumanBodyBones.LeftLowerArm)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightLowerArm, set.factor);
                        else if (bone == HumanBodyBones.LeftUpperLeg)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightUpperLeg, set.factor);
                        else if (bone == HumanBodyBones.LeftLowerLeg)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightLowerLeg, set.factor);
                        else if (bone == HumanBodyBones.LeftHand)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightHand, set.factor);
                        else if (bone == HumanBodyBones.LeftFoot)
                            mgs[i].SetRootBoneRadius(HumanBodyBones.RightFoot, set.factor);
                        mgs[i].SetRootBoneRadius(bone, set.factor);
                    }

                    // set segment
                    _boneReference.GetLeftRightBodyPartTransform(bone, ref _cacheListTrs);
                    for (int index = 0; index < _cacheListTrs.Count; index++)
                    {
                        RootColliderManager manager = _cacheListTrs[index].GetComponent<RootColliderManager>();
                        if (manager != null)
                            manager.SetSegment(set.segment);
                    }
                }

                Color preColor = GUI.color;
                GUI.color = Color.green;
                if (GUILayout.Button("Finish Config", GUILayout.MinHeight(50f)))
                {
                    _linePointCS.DestroyHandlers();
                    _linePointCS._initLineSphereOver = true;
                    RefreshTargetScaleFactor(_targetObj);
                }
                GUI.color = preColor;
            }
            else
            {
                DrawBodyPartLineSphere(HumanBodyBones.Chest);
                DrawBodyPartLineSphere(HumanBodyBones.Spine);
                DrawBodyPartLineSphere(HumanBodyBones.Hips);
                DrawBodyPartLineSphere(HumanBodyBones.Neck);
                DrawBodyPartLineSphere(HumanBodyBones.LeftShoulder);
                DrawBodyPartLineSphere(HumanBodyBones.LeftUpperArm);
                DrawBodyPartLineSphere(HumanBodyBones.LeftLowerArm);
                DrawBodyPartLineSphere(HumanBodyBones.LeftHand);
                DrawBodyPartLineSphere(HumanBodyBones.LeftUpperLeg);
                DrawBodyPartLineSphere(HumanBodyBones.LeftLowerLeg);
                DrawBodyPartLineSphere(HumanBodyBones.LeftFoot);
            }
        }

        private List<Transform> _cacheListTrs = new List<Transform>();

        private string GetBodyBoneName(HumanBodyBones bone)
        {
            string str = bone.ToString();
            if (
                bone == HumanBodyBones.LeftFoot ||
                bone == HumanBodyBones.LeftShoulder ||
                bone == HumanBodyBones.LeftUpperArm ||
                bone == HumanBodyBones.LeftLowerArm ||
                bone == HumanBodyBones.LeftUpperLeg ||
                bone == HumanBodyBones.LeftLowerLeg ||
                bone == HumanBodyBones.LeftHand
                )
            {
                str = str.Replace("Left", "");
            }
            return str;
        }

        private List<int> _cacheBody = new List<int>();
        private void DrawBodyPartLineSphere(HumanBodyBones body)
        {
            _cacheBody.Clear();
            EditorTools.DrawSpace(1);
            Color preColor = GUI.color;
            GUI.color = Color.cyan;
            EditorGUILayout.BeginHorizontal("Box");
            if (!_dicLinePointStatus.ContainsKey(body))
                _dicLinePointStatus[body] = false;

            string settingName = body.ToString();
            _cacheBody.Add((int)body);

            if (body == HumanBodyBones.LeftFoot)
            {
                settingName = "Foot";
                _cacheBody.Add((int)HumanBodyBones.RightFoot);
            }
            else if (body == HumanBodyBones.LeftUpperArm)
            {
                settingName = "Upper Arm";
                _cacheBody.Add((int)HumanBodyBones.RightUpperArm);
            }
            else if (body == HumanBodyBones.LeftUpperLeg)
            {
                settingName = "Upper Leg";
                _cacheBody.Add((int)HumanBodyBones.RightUpperLeg);
            }
            else if (body == HumanBodyBones.LeftShoulder)
            {
                settingName = "Shoulder";
                _cacheBody.Add((int)HumanBodyBones.RightShoulder);
            }
            else if (body == HumanBodyBones.LeftLowerArm)
            {
                settingName = "Lower Arm";
                _cacheBody.Add((int)HumanBodyBones.RightLowerArm);
            }
            else if (body == HumanBodyBones.LeftLowerLeg)
            {
                settingName = "Lower Leg";
                _cacheBody.Add((int)HumanBodyBones.RightLowerLeg);
            }
            else if (body == HumanBodyBones.LeftHand)
            {
                settingName = "Hand";
                _cacheBody.Add((int)HumanBodyBones.RightHand);
            }

            _dicLinePointStatus[body] = EditorGUILayout.Foldout(_dicLinePointStatus[body], (settingName + "Setting"));
            EditorGUILayout.EndHorizontal();
            GUI.color = preColor;
            if (!_dicLinePointStatus[body])
                return;
            EditorTools.DrawSpace(1);
            foreach (var bone in _cacheBody)
            {
                // Get all Line Sphere Content
                if (_boneReference._dicBones.ContainsKey(bone) && _boneReference._dicBones[bone] != null)
                {
                    Transform trs = _boneReference._dicBones[bone];
                    RootColliderManager mg = trs.GetComponent<RootColliderManager>();
                    if (mg == null)
                        continue;

                    List<LineSphereCollider> list = mg._lineSphereColliders;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool isEdit = _currentEditHandlers == list[i];
                        Color color = GUI.color;
                        if (isEdit)
                            GUI.color = Color.yellow;
                        else
                            GUI.color = color;
                        isEdit = EditorGUILayout.Toggle(((HumanBodyBones)bone).ToString() + "->" + list[i].lineIndex.ToString(), isEdit, "Button");
                        if (!isEdit && _currentEditHandlers == list[i])
                        {
                            _currentEditHandlers = null;
                        }
                        if (isEdit && _currentEditHandlers != list[i])
                        {
                            _currentEditHandlers = list[i];
                            if (handler001 == null)
                            {
                                handler001 = new GameObject("Handler001(Clone)", typeof(HandlerContoller));
                            }
                            if (handler002 == null)
                                handler002 = new GameObject("Handler002(Clone)", typeof(HandlerContoller));
                            RefreshHandlerPosition();
                        }

                        GUI.color = Color.green;
                        if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
                        {
                            RootColliderManager targetMg = list[i].gameObject.GetComponent<RootColliderManager>();
                            targetMg.InsertLineSphereCollider(list[i]);
                            RefreshHandlerPosition();
                            break;
                        }
                        GUI.color = Color.red;
                        if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                        {
                            RootColliderManager targetMg = list[i].gameObject.GetComponent<RootColliderManager>();
                            bool delCur = _currentEditHandlers == list[i];
                            if (targetMg.RemoveLineSpereCollider(list[i]) && delCur)
                            {
                                ShutDownHandler();
                            }
                            RefreshHandlerPosition();
                            break;
                        }
                        GUI.color = color;
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void RefreshHandlerPosition()
        {
            if (handler001 != null && _currentEditHandlers != null)
                handler001.transform.position = _currentEditHandlers.WorldA;
            if (handler002 != null && _currentEditHandlers != null)
                handler002.transform.position = _currentEditHandlers.WorldB;
        }

        private void DrawNormalModeColliderSettings()
        {
            EditorTools.DrawSpace(1);
            EditorTools.DrawLabelWithColorInBox("Wrap Normal Body Collider Settings", Color.green);

            EditorTools.DrawSpace(1);
            _settings.Spine = EditorGUILayout.Toggle("Spine", _settings.Spine);
            _settings.Chest = EditorGUILayout.Toggle("Chest", _settings.Chest);

            if (DrawPartSettingFold(HumanBodyBones.Head))
            {
                _settings._Head._overLap = EditorGUILayout.Slider("HeadOverLap", _settings._Head._overLap, -2f, 4f);
                _settings._Head._widthFactor = EditorGUILayout.Slider("HeadWidthFactor", _settings._Head._widthFactor, 0.1f, 2f);
                _settings._Head._offSet.x = EditorGUILayout.Slider("OffSetX", _settings._Head._offSet.x, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Head._offSet.y = EditorGUILayout.Slider("OffSetY", _settings._Head._offSet.y, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Head._offSet.z = EditorGUILayout.Slider("OffSetZ", _settings._Head._offSet.z, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                EditorTools.DrawSpace(1);
            }

            if (DrawPartSettingFold(HumanBodyBones.Chest))
            {
                _settings._Chest._overLap = EditorGUILayout.Slider("ChestOverLap", _settings._Chest._overLap, -2f, 4f);
                _settings._Chest._widthFactor = EditorGUILayout.Slider("ChestWidthFactor", _settings._Chest._widthFactor, 0.1f, 2f);
                _settings._Chest._offSet.x = EditorGUILayout.Slider("OffSetX", _settings._Chest._offSet.x, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Chest._offSet.y = EditorGUILayout.Slider("OffSetY", _settings._Chest._offSet.y, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Chest._offSet.z = EditorGUILayout.Slider("OffSetZ", _settings._Chest._offSet.z, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                EditorTools.DrawSpace(1);
            }

            if (DrawPartSettingFold(HumanBodyBones.Spine))
            {
                _settings._Spine._overLap = EditorGUILayout.Slider("SpineOverLap", _settings._Spine._overLap, -2f, 4f);
                _settings._Spine._widthFactor = EditorGUILayout.Slider("SpineWidthFactor", _settings._Spine._widthFactor, 0.1f, 2f);
                _settings._Spine._offSet.x = EditorGUILayout.Slider("OffSetX", _settings._Spine._offSet.x, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Spine._offSet.y = EditorGUILayout.Slider("OffSetY", _settings._Spine._offSet.y, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Spine._offSet.z = EditorGUILayout.Slider("OffSetZ", _settings._Spine._offSet.z, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                EditorTools.DrawSpace(1);
            }

            if (DrawPartSettingFold(HumanBodyBones.Hips))
            {
                _settings._Hip._overLap = EditorGUILayout.Slider("HipOverLap", _settings._Hip._overLap, -2f, 4f);
                _settings._Hip._widthFactor = EditorGUILayout.Slider("SpineWidthFactor", _settings._Hip._widthFactor, 0.0f, 2f);
                _settings._Hip._offSet.x = EditorGUILayout.Slider("OffSetX", _settings._Hip._offSet.x, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Hip._offSet.y = EditorGUILayout.Slider("OffSetY", _settings._Hip._offSet.y, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                _settings._Hip._offSet.z = EditorGUILayout.Slider("OffSetZ", _settings._Hip._offSet.z, NormalColliderSettings.MinOffSet, NormalColliderSettings.MaxOffSet);
                EditorTools.DrawSpace(1);
            }

            if (DrawPartSettingFold(HumanBodyBones.LeftUpperArm))
            {
                _settings._Arm._overLap = EditorGUILayout.Slider("ArmOverLap", _settings._Arm._overLap, -2f, 4f);
                _settings._Arm._widthFactor = EditorGUILayout.Slider("ArmWidthFactor", _settings._Arm._widthFactor, 0.01f, 2f);
                EditorTools.DrawSpace(1);
            }

            if (DrawPartSettingFold(HumanBodyBones.LeftUpperLeg))
            {
                _settings._Leg._overLap = EditorGUILayout.Slider("LegOverLap", _settings._Leg._overLap, -2f, 4f);
                _settings._Leg._widthFactor = EditorGUILayout.Slider("LegWidthFactor", _settings._Leg._widthFactor, 0.01f, 2f);
                EditorTools.DrawSpace(1);
            }
        }

        private bool DrawPartSettingFold(HumanBodyBones de)
        {
            Color preColor = GUI.color;
            GUI.color = Color.cyan;
            EditorGUILayout.BeginHorizontal("Box");
            if (!_dicFoldStatus.ContainsKey(de))
                _dicFoldStatus[de] = false;

            string name = de.ToString();
            if (de == HumanBodyBones.LeftUpperLeg)
                name = "Leg";
            else if (de == HumanBodyBones.LeftUpperArm)
                name = "Arm";
            _dicFoldStatus[de] = EditorGUILayout.Foldout(_dicFoldStatus[de], name + " Setting");
            EditorGUILayout.EndHorizontal();
            GUI.color = preColor;
            return _dicFoldStatus[de];
        }

        private void ReMappingWithAvatar()
        {
            Animator animator = _targetObj.GetComponent<Animator>();
            if (animator != null && animator.isHuman && animator.avatar != null)
            {
                _boneReference.MapHumanAvatarToBoneReferences(this._targetObj.transform, animator);
            }
            else
            {
                EditorUtility.DisplayDialog("Information",
                    "Target Must have Animator component and animation type must be Humanoid, animator's Avatar can't be Null",
                    "OK");
                _boneReference.ResetReference();
            }
        }

        private void ShowBoneReference(HumanBodyBoneReferenceData refercence)
        {
            if (refercence == null)
                return;
            for (int i = 0; i < refercence._listMappingBoneName.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int bone = refercence._listMappingBoneName[i];
                Transform trs = refercence._dicBones[bone];
                trs = EditorGUILayout.ObjectField(((HumanBodyBones)bone).ToString(), trs, typeof(Transform), true) as Transform;
                refercence._dicBones[bone] = trs;
                if (trs == null)
                    DrawBoneMarkSign(Color.red, "✘");
                else
                    DrawBoneMarkSign(Color.green, "✔");
                EditorGUILayout.EndHorizontal();
            }
        }

        public class NormalColliderSettings
        {
            public bool Chest = true;
            public bool Spine = true;
            public SubNormalColliderSetting _Head;
            public SubNormalColliderSetting _Chest;
            public SubNormalColliderSetting _Spine;
            public SubNormalColliderSetting _Hip;
            public SubNormalColliderSetting _Leg;
            public SubNormalColliderSetting _Arm;
            public const float MaxOffSet = (20);
            public const float MinOffSet = (-20);
            public const float OffSetFactor = 0.01f;

            public NormalColliderSettings()
            {
                _Hip = new SubNormalColliderSetting(0.0f, 0.75f);
                _Spine = new SubNormalColliderSetting(0.0f, 0.75f);
                _Chest = new SubNormalColliderSetting(0.0f, 0.75f);
                _Leg = new SubNormalColliderSetting(0.0f, 0.1f);
                _Arm = new SubNormalColliderSetting(0.0f, 0.1f);
                _Head = new SubNormalColliderSetting(0.0f, 1f);
            }
        }

        public class LineSphereColliderSettings
        {
            public bool Chest = true;
            public bool Spine = true;
            public bool Hip = true;
            public bool Neck = true;
            public bool _initLineSphereOver = false;

            public Dictionary<HumanBodyBones, LineSphereColliderSubSetting> _settings = new Dictionary<HumanBodyBones, LineSphereColliderSubSetting>()
            {
                { HumanBodyBones.Head, new LineSphereColliderSubSetting()},
                { HumanBodyBones.Neck, new LineSphereColliderSubSetting()},
                { HumanBodyBones.Chest, new LineSphereColliderSubSetting()},
                { HumanBodyBones.Spine, new LineSphereColliderSubSetting()},
                { HumanBodyBones.Hips, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftShoulder, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftUpperArm, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftLowerArm, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftHand, new LineSphereColliderSubSetting() },
                { HumanBodyBones.LeftUpperLeg, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftLowerLeg, new LineSphereColliderSubSetting()},
                { HumanBodyBones.LeftFoot, new LineSphereColliderSubSetting()},
            };

            public LineSphereColliderSubSetting GetSubSetting(HumanBodyBones bone)
            {
                if (_settings.ContainsKey(bone))
                    return _settings[bone];
                if (bone == HumanBodyBones.RightShoulder)
                    return _settings[HumanBodyBones.LeftShoulder];
                else if (bone == HumanBodyBones.RightUpperArm)
                    return _settings[HumanBodyBones.LeftUpperArm];
                else if (bone == HumanBodyBones.RightLowerArm)
                    return _settings[HumanBodyBones.LeftLowerArm];
                else if (bone == HumanBodyBones.RightUpperLeg)
                    return _settings[HumanBodyBones.LeftUpperLeg];
                else if (bone == HumanBodyBones.RightLowerLeg)
                    return _settings[HumanBodyBones.LeftLowerLeg];
                else if (bone == HumanBodyBones.RightHand)
                    return _settings[HumanBodyBones.LeftHand];
                else if (bone == HumanBodyBones.RightFoot)
                    return _settings[HumanBodyBones.LeftFoot];
                return null;
            }

            public Dictionary<HumanBodyBones, Transform> _dicRootBoneHandler = new Dictionary<HumanBodyBones, Transform>();
            public void DestroyHandlers()
            {
                foreach (var handler in _dicRootBoneHandler)
                {
                    if (handler.Value != null)
                    {
                        GameObject.DestroyImmediate(handler.Value.gameObject);
                    }
                }
                _dicRootBoneHandler.Clear();
            }

            public void CreateHandlers(HumanBodyBoneReferenceData referenceData)
            {
                DestroyHandlers();
                foreach (var bone in referenceData._dicBones)
                {
                    if (bone.Value != null)
                    {
                        GameObject ob = bone.Value.gameObject;
                        GameObject newHandler = new GameObject("Handler_" + ((HumanBodyBones)bone.Key).ToString());
                        newHandler.transform.position = bone.Value.transform.position;
                        newHandler.transform.rotation = Quaternion.identity;
                        HandlerContoller hc = newHandler.AddComponent<HandlerContoller>();
                        hc._targetBones = (HumanBodyBones)bone.Key;
                        _dicRootBoneHandler[(HumanBodyBones)bone.Key] = newHandler.transform;
                    }
                }
            }
        }

        public class LineSphereColliderSubSetting
        {
            public float factor = 1.0f;
            public int segment = 1;
        }

        private LineSphereColliderSettings _linePointCS = new LineSphereColliderSettings();
        public class SubNormalColliderSetting
        {
            public float _overLap = 0.01f;
            public float _widthFactor = 0.75f;
            public Vector3 _offSet = Vector3.zero;

            public SubNormalColliderSetting(float overLap, float widthFactor)
            {
                _overLap = overLap;
                _widthFactor = widthFactor;
            }
        }

        private static NormalColliderSettings _settings = new NormalColliderSettings();
        private void ClearNormalColliders()
        {
            if (_targetObj == null || !_boneReference.IsValid())
                return;
            foreach (var bone in _boneReference._dicBones)
            {
                Transform trs = bone.Value;
                if (trs != null)
                {
                    Collider collider = trs.GetComponent<Collider>();
                    if (collider != null)
                        DestroyImmediate(collider);
                }
            }
        }

        private void AutoCreateColliderForHumanBody(Transform root, HumanBodyBoneReferenceData r, NormalColliderSettings settings)
        {
            ClearNormalColliders();
            // create main body
            Transform trsHead = r._dicBones[(int)HumanBodyBones.Head];
            Transform trsLeftUpperArm = r._dicBones[(int)HumanBodyBones.LeftUpperArm];
            Transform trsRightUpperArm = r._dicBones[(int)HumanBodyBones.RightUpperArm];
            Transform trsChest = r._dicBones[(int)HumanBodyBones.Chest];
            Transform trsSpine = r._dicBones[(int)HumanBodyBones.Spine];

            Vector3 upperArmToHeadCentroid = Vector3.Lerp(trsLeftUpperArm.position, trsRightUpperArm.position, 0.5f);
            upperArmToHeadCentroid = Vector3.Lerp(upperArmToHeadCentroid, trsHead.position, 0.5f);
            Vector3 v2 = trsLeftUpperArm.position - trsRightUpperArm.position;
            float torsoWidth = v2.magnitude;

            Transform trsHip = r._dicBones[(int)HumanBodyBones.Hips];
            Vector3 hipsStartPoint = trsHip.position;
            float toHead = Vector3.Distance(trsHead.position, root.position);
            float toHips = Vector3.Distance(trsHip.position, root.position);
            if (toHips < toHead * 0.2f)
            {
                hipsStartPoint = Vector3.Lerp(
                    r._dicBones[(int)HumanBodyBones.LeftUpperLeg].position,
                    r._dicBones[(int)HumanBodyBones.RightUpperLeg].position,
                    0.5f
                    );
            }

            Vector3 lastEndPoint = settings.Spine && trsSpine != null ? trsSpine.position : (settings.Chest && trsChest != null ? trsChest.position : upperArmToHeadCentroid);
            hipsStartPoint += (hipsStartPoint - upperArmToHeadCentroid) * 0.1f;
            float hipsWidth = settings.Spine || settings.Chest ? torsoWidth * 0.8f : torsoWidth;
            CreateManualCollider(
                trsHip,
                hipsStartPoint,
                lastEndPoint,
                ColliderType.Capsule,
                settings._Hip._overLap,
                hipsWidth * 0.5f * settings._Hip._widthFactor,
                settings._Hip._offSet * NormalColliderSettings.OffSetFactor);

            // create spine
            if (settings.Spine && trsSpine != null)
            {
                Vector3 spineStartPos = lastEndPoint;
                lastEndPoint = (settings.Chest && trsChest != null) ? trsChest.position : upperArmToHeadCentroid;
                float spineWidth = (settings.Chest && trsChest != null) ? torsoWidth * 0.75f : torsoWidth;
                CreateManualCollider(
                    trsSpine,
                    spineStartPos,
                    lastEndPoint,
                    ColliderType.Capsule,
                    settings._Spine._overLap, spineWidth * 0.5f * settings._Spine._widthFactor,
                    settings._Spine._offSet * NormalColliderSettings.OffSetFactor);
            }

            // create chest
            if (settings.Chest && trsChest != null)
            {
                Vector3 chestStartPoint = lastEndPoint;
                lastEndPoint = upperArmToHeadCentroid;
                CreateManualCollider(
                    trsChest,
                    chestStartPoint,
                    lastEndPoint,
                    ColliderType.Capsule,
                    settings._Chest._overLap,
                    torsoWidth * 0.5f * settings._Chest._widthFactor,
                    settings._Chest._offSet * NormalColliderSettings.OffSetFactor);
            }

            // create head
            Vector3 headStartPoint = lastEndPoint;
            Vector3 headEndPoint = headStartPoint + (headStartPoint - hipsStartPoint) * 0.45f;
            Vector3 axis = trsHead.TransformVector(GetAxisVectorToDirection(trsHead, headEndPoint - headStartPoint));
            headEndPoint = headStartPoint + Vector3.Project(headEndPoint - headStartPoint, axis).normalized * (headEndPoint - headStartPoint).magnitude;
            float headWidth = Vector3.Distance(headEndPoint, headStartPoint) * 0.75f;
            CreateManualCollider(
                trsHead,
                headStartPoint,
                headEndPoint,
                ColliderType.Capsule,
                settings._Head._overLap,
                headWidth * 0.5f * settings._Head._widthFactor,
                _settings._Head._offSet * NormalColliderSettings.OffSetFactor);

            // create Arm colliders
            float dis01 = 0.04f;
            float dis02 = 0.04f;
            Transform trs01 = r._dicBones[(int)HumanBodyBones.LeftUpperArm];
            Transform trs02 = r._dicBones[(int)HumanBodyBones.LeftLowerArm];
            Transform trs03 = r._dicBones[(int)HumanBodyBones.LeftHand];
            dis01 = Vector3.Distance(trs01.position, trs02.position);
            dis02 = Vector3.Distance(trs02.position, trs03.position);
            CreateManualCollider(trs01, trs01.position, trs02.position, ColliderType.Capsule, settings._Arm._overLap, dis01 * settings._Arm._widthFactor);
            CreateManualCollider(trs02, trs02.position, trs03.position, ColliderType.Capsule, settings._Arm._overLap, dis01 * settings._Arm._widthFactor);

            trs01 = r._dicBones[(int)HumanBodyBones.RightUpperArm];
            trs02 = r._dicBones[(int)HumanBodyBones.RightLowerArm];
            trs03 = r._dicBones[(int)HumanBodyBones.RightHand];
            dis01 = Vector3.Distance(trs01.position, trs02.position);
            dis02 = Vector3.Distance(trs02.position, trs03.position);
            CreateManualCollider(trs01, trs01.position, trs02.position, ColliderType.Capsule, settings._Arm._overLap, dis01 * settings._Arm._widthFactor);
            CreateManualCollider(trs02, trs02.position, trs03.position, ColliderType.Capsule, settings._Arm._overLap, dis01 * settings._Arm._widthFactor);

            // create leg colliders
            trs01 = r._dicBones[(int)HumanBodyBones.LeftUpperLeg];
            trs02 = r._dicBones[(int)HumanBodyBones.LeftLowerLeg];
            trs03 = r._dicBones[(int)HumanBodyBones.LeftFoot];
            dis01 = Vector3.Distance(trs01.position, trs02.position);
            dis02 = Vector3.Distance(trs02.position, trs03.position);
            CreateManualCollider(trs01, trs01.position, trs02.position, ColliderType.Capsule, settings._Leg._overLap, dis01 * settings._Leg._widthFactor);
            CreateManualCollider(trs02, trs02.position, trs03.position, ColliderType.Capsule, settings._Leg._overLap, dis01 * settings._Leg._widthFactor);

            trs01 = r._dicBones[(int)HumanBodyBones.RightUpperLeg];
            trs02 = r._dicBones[(int)HumanBodyBones.RightLowerLeg];
            trs03 = r._dicBones[(int)HumanBodyBones.RightFoot];
            dis01 = Vector3.Distance(trs01.position, trs02.position);
            dis02 = Vector3.Distance(trs02.position, trs03.position);
            CreateManualCollider(trs01, trs01.position, trs02.position, ColliderType.Capsule, settings._Leg._overLap, dis01 * settings._Leg._widthFactor);
            CreateManualCollider(trs02, trs02.position, trs03.position, ColliderType.Capsule, settings._Leg._overLap, dis01 * settings._Leg._widthFactor);
        }

        public enum ColliderType
        {
            Box,
            Capsule,
        }

        private void CreateManualCollider(Transform start, Vector3 startPos, Vector3 endPos, ColliderType type, float overLap, float width, Vector3 offSet)
        {
            Vector3 dir = endPos - startPos;
            float f = Mathf.Clamp(overLap, -2f, 4f);
            float dis = dir.magnitude * (1f + f);
            Vector3 v = start.InverseTransformDirection(dir);

            if (type == ColliderType.Capsule)
            {
                float dotx = Mathf.Abs(Vector3.Dot(v, start.InverseTransformDirection(start.right)));
                float doty = Mathf.Abs(Vector3.Dot(v, start.InverseTransformDirection(start.up)));
                float dotz = Mathf.Abs(Vector3.Dot(v, start.InverseTransformDirection(start.forward)));

                int result = 0;
                if (doty > dotz && doty > dotx) result = 1;
                if (dotz > dotx && dotz > doty) result = 2;

                CapsuleCollider collider = start.GetComponent<CapsuleCollider>();
                if (collider == null)
                    collider = start.gameObject.AddComponent<CapsuleCollider>();
                collider.direction = result;
                collider.radius = width;
                collider.height = dis;
                collider.center = start.InverseTransformPoint(Vector3.Lerp(startPos, endPos, 0.5f) + (start.forward * offSet.z) + start.right * offSet.x + start.up * offSet.y);
            }
            else if (type == ColliderType.Box)
            {
            }

        }
        private void CreateManualCollider(Transform start, Vector3 startPos, Vector3 endPos, ColliderType type, float overLap, float width)
        {
            CreateManualCollider(start, startPos, endPos, type, overLap, width, Vector3.zero);
        }

        private void DrawBoneMarkSign(Color color, string text)
        {
            Color preColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField(text, GUILayout.MaxWidth(20));
            GUI.color = preColor;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_targetObj == null)
                return;

            Renderer[] render = _targetObj.GetComponentsInChildren<Renderer>();
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(sceneView.camera);
            bool visible = false;
            for (int i = 0; i < render.Length; i++)
            {
                if (GeometryUtility.TestPlanesAABB(planes, render[i].bounds))
                {
                    visible = true;
                    break;
                }
            }
            if (visible)
            {
                ShowBonesGizmos();
            }

            // Draw setting Handlers
            if (_mode == AutoWrapColliderMode.LineSphereFakeCollider && !_linePointCS._initLineSphereOver)
            {
                GameObject sel = Selection.activeGameObject;
                RootColliderManager[] mgs = _targetObj.GetComponentsInChildren<RootColliderManager>();
                foreach (var handler in _linePointCS._dicRootBoneHandler)
                {
                    Handles.SphereHandleCap(-1, handler.Value.position, Quaternion.identity, 0.06f, EventType.Repaint);
                    for (int i = 0; i < mgs.Length; i++)
                    {
                        mgs[i].SetRootBonePosition(handler.Key, handler.Value.position);
                    }
                }

                if (sel != null)
                {
                    HandlerContoller hc = sel.GetComponent<HandlerContoller>();
                    if (hc != null)
                    {
                        LineSphereColliderSubSetting setting = _linePointCS.GetSubSetting(hc._targetBones);
                        if (setting != null)
                        {
                            Handles.BeginGUI();
                            Rect rectScene = sceneView.position;
                            float height = 150f;
                            float width = 200f;
                            Rect rect = new Rect(rectScene.width - width, rectScene.height - height, width - 5, height - 25);
                            Color preColor = GUI.color;
                            GUILayout.BeginArea(rect, GUIContent.none, "CurveEditorBackground");
                            GUILayout.Label(GetBodyBoneName(hc._targetBones));
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Radius");
                            // when using EditorGUILayout.Draw can't move Handler......
                            // EditorGUILayout.Slider(setting.factor, 0.01f, 5f, GUILayout.MinWidth(50f));
                            setting.factor = GUILayout.HorizontalSlider(setting.factor, 0.01f, 5f, GUILayout.MinWidth(50f));
                            Debug.Log("####" + setting.factor);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUI.Label(new Rect(0, 60, 60, 20), "Segment");
                            //setting.segment = EditorGUILayout.IntSlider(setting.segment, 1, 6);
                            setting.segment = EditorGUI.IntSlider(new Rect(60, 60, 120, 20), setting.segment, 1, 6);
                            GUILayout.EndHorizontal();
                            GUILayout.EndArea();
                            Handles.EndGUI();
                            Repaint();
                        }
                    }
                }
            }

            // Handler
            if (_mode == AutoWrapColliderMode.LineSphereFakeCollider && _currentEditHandlers != null
                && handler001 != null
                && handler002 != null
                )
            {
                RootColliderManager mg = _currentEditHandlers.GetComponent<RootColliderManager>();
                Handles.SphereHandleCap(-1, handler001.transform.position, Quaternion.identity, 0.06f, EventType.Repaint);
                Handles.SphereHandleCap(-1, handler002.transform.position, Quaternion.identity, 0.06f, EventType.Repaint);

                Handles.BeginGUI();
                Rect rectScene = sceneView.position;
                float height = 150f;
                float width = 200f;
                Rect rect = new Rect(rectScene.width - width, rectScene.height - height, width - 5, height - 25);
                Color preColor = GUI.color;
                if (EditorGUIUtility.isProSkin)
                    GUILayout.BeginArea(rect, GUIContent.none, "CurveEditorBackground");
                else
                {
                    GUILayout.BeginArea(rect,EditorStyles.helpBox);
                }

                _handlerRelevanceAPos = EditorGUILayout.Toggle(aPosGUIContent, _handlerRelevanceAPos, GUILayout.MinWidth(100));
                _handlerRelevanceARadius = EditorGUILayout.Toggle(aRadiusGUIContent, _handlerRelevanceARadius);
                GUILayout.BeginHorizontal();
                GUI.color = Color.blue;
                GUILayout.Label("PointA");
                float aRadius = GUILayout.HorizontalSlider(_currentEditHandlers.WorldRadiusA * 100f, 0.1f, 30f, GUILayout.MinWidth(50f)) * 0.01f;
                GUILayout.Label(aRadius.ToString("0.00"));
                GUILayout.EndHorizontal();

                GUI.color = preColor;
                EditorTools.DrawSpace(2);
                _handlerRelevanceBPos = EditorGUILayout.Toggle(aPosGUIContent, _handlerRelevanceBPos);
                _handlerRelevanceBRadius = EditorGUILayout.Toggle(aRadiusGUIContent, _handlerRelevanceBRadius);

                GUILayout.BeginHorizontal();
                GUI.color = Color.blue;
                GUILayout.Label("PointB");

                float bRadius = GUILayout.HorizontalSlider(_currentEditHandlers.WorldRadiusB * 100f, 0.1f, 30f, GUILayout.MinWidth(50f)) * 0.01f;
                GUILayout.Label(bRadius.ToString("0.00"));
                GUILayout.EndHorizontal();
                GUI.color = preColor;
                GUILayout.EndArea();
                Handles.EndGUI();

                mg.SetWorldPosWithHandler(_currentEditHandlers, handler001.transform.position, aRadius, handler002.transform.position, bRadius, true);
                // set all relevance root bone
                if (!(_handlerRelevanceAPos || _handlerRelevanceARadius || _handlerRelevanceBPos || _handlerRelevanceBRadius))
                    return;

                RootColliderManager[] mgs = _targetObj.GetComponentsInChildren<RootColliderManager>();
                for (int i = 0; i < mgs.Length; i++)
                {
                    if (_handlerRelevanceAPos) { mgs[i].SetRootBonePosition(_currentEditHandlers._startBone, _currentEditHandlers.WorldA); }
                    if (_handlerRelevanceBPos) { mgs[i].SetRootBonePosition(_currentEditHandlers._endBone, _currentEditHandlers.WorldB); }
                    if (_handlerRelevanceARadius) { mgs[i].SetRootBoneRadius(_currentEditHandlers._startBone, _currentEditHandlers.RadiusA, false); }
                    if (_handlerRelevanceBRadius) { mgs[i].SetRootBoneRadius(_currentEditHandlers._endBone, _currentEditHandlers.RadiusB, false); }
                }
            }
        }

        private bool _handlerRelevanceAPos = false;
        private static GUIContent aPosGUIContent = new GUIContent("EnablePos Rel", " When move Point Position also change the Relevanced Bone Point");
        private bool _handlerRelevanceARadius = false;
        private static GUIContent aRadiusGUIContent = new GUIContent("EnableRad Rel", " When change Point Radius also change the Relevanced Bone Radius");
        private bool _handlerRelevanceBPos = false;
        private bool _handlerRelevanceBRadius = false;

        private bool _showBones = false;
        private void ShowBonesGizmos()
        {
            if (!_showBones || _boneReference == null || _targetObj == null || _targetObj.activeSelf == false)
                return;
            if (Event.current.type == EventType.Repaint)
            {
                foreach (var bone in _boneReference._dicBones)
                {
                    if (bone.Value != null)
                    {
                        Handles.color = Color.gray;
                        Handles.SphereHandleCap(-1, bone.Value.position, bone.Value.rotation, 0.06f, EventType.Repaint);
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.green;
                        Handles.Label(bone.Value.position, new GUIContent(((HumanBodyBones)bone.Key).ToString()), style);
                    }
                }
            }
        }

        private Vector3 GetAxisVectorToDirection(Transform t, Vector3 direction)
        {
            direction = direction.normalized;
            Vector3 axis = Vector3.right;

            float dotX = Mathf.Abs(Vector3.Dot(Vector3.Normalize(t.right), direction));
            float dotY = Mathf.Abs(Vector3.Dot(Vector3.Normalize(t.up), direction));
            if (dotY > dotX) axis = Vector3.up;
            float dotZ = Mathf.Abs(Vector3.Dot(Vector3.Normalize(t.forward), direction));
            if (dotZ > dotX && dotZ > dotY) axis = Vector3.forward;

            return axis;
        }
    }
}