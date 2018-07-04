using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class RootColliderManager : MonoBehaviour
    {
        public List<LineSphereCollider> _lineSphereColliders = new List<LineSphereCollider>();
        private List<LineSphereCollider> _cacheList = new List<LineSphereCollider>();

        public void InitRootColliderManagerWhenCreate()
        {
            LineSphereCollider[] collider = GetComponents<LineSphereCollider>();
            for (int i = 0; i < collider.Length; i++)
            {
                LineSphereCollider cd = collider[i];
                cd.lineIndex = (i + 1) * 100;
                _lineSphereColliders.Add(cd);
            }
        }

        public void SetWorldPosWithHandler(LineSphereCollider lsc, Vector3 newAPos, float newARadius, Vector3 newBPos, float newBRadius, bool relevance = true)
        {
            if (lsc == null)
                return;
            int count = GetLineCount(lsc.lineIndex);

            lsc.WorldA = newAPos;
            lsc.WorldRadiusA = newARadius;
            lsc.WorldB = newBPos;
            lsc.WorldRadiusB = newBRadius;

            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                if (_lineSphereColliders[i] == lsc)
                {
                    if (count <= 1)
                        break;

                    int a = lsc.lineIndex / 100 * 100;
                    bool first = (lsc.lineIndex % 100 == 0);
                    bool last = (lsc.lineIndex == (a + count - 1));
                    if (first)
                    {
                        LineSphereCollider lscNext = _lineSphereColliders[i + 1];
                        lscNext.WorldA = newBPos;
                        lscNext.WorldRadiusA = newBRadius;
                    }
                    else if (last)
                    {
                        LineSphereCollider lscPre = _lineSphereColliders[i - 1];
                        lscPre.WorldB = newAPos;
                        lscPre.WorldRadiusB = newARadius;
                    }
                    else
                    {
                        LineSphereCollider lscPre = _lineSphereColliders[i - 1];
                        LineSphereCollider lscNext = _lineSphereColliders[i + 1];

                        lscPre.WorldB = newAPos;
                        lscPre.WorldRadiusB = newARadius;
                        lscNext.WorldA = newBPos;
                        lscNext.WorldRadiusA = newBRadius;
                    }
                }
            }
        }

        private void SortLine()
        {
            _lineSphereColliders.Sort((left, right) =>
            {
                if (left.lineIndex > right.lineIndex)
                    return 1;
                else if (left.lineIndex < right.lineIndex)
                    return -1;
                else
                    return 0;
            });

            int index = 0;
            int preIndex = 1;
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                LineSphereCollider lsc = _lineSphereColliders[i];
                if (preIndex != lsc.lineIndex / 100)
                {
                    index = 0;
                    preIndex++;
                }
                lsc.lineIndex = preIndex * 100 + index;
                index++;
            }
        }

        public void SetRootBoneRadius(HumanBodyBones bone, float radius, bool isEditor = true)
        {
            if (bone == HumanBodyBones.LastBone)
                return;
            foreach (var lsc in _lineSphereColliders)
            {
                lsc.SetBoneScaleFactor(bone, radius, isEditor);
            }
        }

        public void SetRootBonePosition(HumanBodyBones bone, Vector3 pos)
        {
            if (bone == HumanBodyBones.LastBone)
                return;

            foreach (var lsc in _lineSphereColliders)
            {
                lsc.SetBonePosition(bone, pos);
            }
        }

        private LineSphereCollider GetLineSphere(int index)
        {
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                if (_lineSphereColliders[i].lineIndex == index)
                    return _lineSphereColliders[i];
            }
            return null;
        }

        private void GetLineSphere(int index, ref List<LineSphereCollider> list)
        {
            if (list == null)
                return;
            list.Clear();
            SortLine();
            _cacheList.Clear();
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                LineSphereCollider lsc = _lineSphereColliders[i];
                if ((index / 100) == (lsc.lineIndex / 100))
                    list.Add(lsc);
            }
        }

        private int GetLineCount(int index)
        {
            int count = 0;
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                if (_lineSphereColliders[i].lineIndex / 100 == index / 100)
                    count++;
            }
            return count;
        }

        public void SetSegment(int segment)
        {
            _cacheList.Clear();
            bool change = false;
            for (int i = 1; i <= _lineSphereColliders.Count; i++)
            {
                int count = GetLineCount(i * 100);
                if (count == 0)
                    continue;
                int value = Mathf.Abs(segment - count);
                bool add = segment > count;
                for (int index = 0; index < value; index++)
                {
                    LineSphereCollider lsc = GetLineSphere(i * 100);
                    if (add)
                        InsertLineSphereCollider(lsc);
                    else
                        RemoveLineSpereCollider(lsc);
                    change = true;
                }
            }

            if (change)
            {
                ResetPointToHypodispersion();
            }
        }

        private void ResetPointToHypodispersion()
        {
            SortLine();
            int preIndex = 1;
            _cacheList.Clear();
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                LineSphereCollider lsc = _lineSphereColliders[i];
                if (preIndex != lsc.lineIndex / 100)
                {
                    // ReCalculate Position
                    CalculatePointToHypodispersion(_cacheList);
                    preIndex++;
                    _cacheList.Clear();
                }
                _cacheList.Add(lsc);
            }
            if (_cacheList.Count > 0)
                CalculatePointToHypodispersion(_cacheList);
        }

        private void CalculatePointToHypodispersion(List<LineSphereCollider> list)
        {
            // ReCalculate Position
            int cacheCount = _cacheList.Count;
            if (cacheCount > 1)
            {
                LineSphereCollider start = _cacheList[0];
                LineSphereCollider end = _cacheList[cacheCount - 1];
                Vector3 startPos = start.WorldA;
                Vector3 endPos = end.WorldB;
                float radius = Mathf.Min(start.RadiusA, end.RadiusB);
                float d = 1f / (cacheCount);
                for (int cIndex = 0; cIndex < cacheCount; cIndex++)
                {
                    LineSphereCollider tlsc = _cacheList[cIndex];
                    tlsc.WorldA = Vector3.Lerp(startPos, endPos, cIndex * d);
                    tlsc.WorldB = Vector3.Lerp(startPos, endPos, (cIndex + 1) * d);
                    tlsc.RadiusA = radius;
                    tlsc.RadiusB = radius + 0.001f;
                }
            }
        }

        public void InsertLineSphereCollider(LineSphereCollider cd)
        {
            SortLine();
            bool insertOver = false;
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                LineSphereCollider lsc = _lineSphereColliders[i];
                if (lsc == cd)
                {
                    Vector3 center = Vector3.Lerp(lsc.WorldA, lsc.WorldB, 0.5f);
                    LineSphereCollider newlsc = this.gameObject.AddComponent<LineSphereCollider>();

                    newlsc.WorldA = center;
                    newlsc.WorldRadiusA = lsc.WorldRadiusA;
                    newlsc.EditorAFactor = lsc.EditorAFactor;
                    newlsc.scaleAFactor = lsc.scaleAFactor;

                    newlsc.WorldRadiusB = lsc.WorldRadiusB;
                    newlsc.WorldB = lsc.WorldB;
                    newlsc.EditorBFactor = lsc.EditorBFactor;
                    newlsc.scaleBFactor = lsc.scaleBFactor;
                    newlsc._endBone = lsc._endBone;

                    lsc.WorldB = newlsc.WorldA;
                    lsc.WorldRadiusB = newlsc.WorldRadiusB;
                    lsc.EditorBFactor = newlsc.EditorBFactor;
                    lsc.scaleBFactor = newlsc.scaleBFactor;
                    lsc._endBone = newlsc._startBone;
                    newlsc.lineIndex = lsc.lineIndex + 1;
                    _lineSphereColliders.Insert(i + 1, newlsc);
                    i++;
                    insertOver = true;
                }
                else if (insertOver)
                {
                    lsc.lineIndex++;
                }
            }
            SortLine();
        }

        public bool RemoveLineSpereCollider(LineSphereCollider inlsc)
        {
            SortLine();
            int count = GetLineCount(inlsc.lineIndex);
            for (int i = 0; i < _lineSphereColliders.Count; i++)
            {
                LineSphereCollider lsc = _lineSphereColliders[i];
                if (lsc == inlsc)
                {
                    if (count <= 1)
                        return false;
                    int a = inlsc.lineIndex / 100 * 100;
                    bool first = (inlsc.lineIndex % 100 == 0);
                    bool last = (inlsc.lineIndex == (a + count - 1));
                    if (first)
                    {
                        LineSphereCollider lscNext = _lineSphereColliders[i + 1];
                        inlsc._endBone = lscNext._endBone;
                        inlsc.WorldB = lscNext.WorldB;
                        inlsc.WorldRadiusB = lscNext.WorldRadiusB;
                        inlsc.EditorBFactor = lscNext.EditorBFactor;
                        inlsc.scaleBFactor = lscNext.scaleBFactor;
                        _lineSphereColliders.Remove(lscNext);
                        GameObject.DestroyImmediate(lscNext);
                    }
                    else if (last)
                    {
                        LineSphereCollider lscPre = _lineSphereColliders[i - 1];
                        inlsc._startBone = lscPre._startBone;
                        inlsc.WorldA = lscPre.WorldA;
                        inlsc.WorldRadiusA = lscPre.WorldRadiusA;
                        inlsc.EditorAFactor = lscPre.EditorAFactor;
                        inlsc.scaleAFactor = lscPre.scaleAFactor;
                        _lineSphereColliders.Remove(lscPre);
                        GameObject.DestroyImmediate(lscPre);
                    }
                    else
                    {
                        LineSphereCollider lscPre = _lineSphereColliders[i - 1];
                        LineSphereCollider lscNext = _lineSphereColliders[i + 1];

                        lscNext._startBone = lscPre._endBone;
                        lscNext.WorldA = lscPre.WorldB;
                        lscNext.EditorAFactor = lscPre.EditorBFactor;
                        lscNext.scaleAFactor = lscPre.scaleBFactor;
                        lscNext.WorldRadiusA = lscPre.WorldRadiusB;
                        _lineSphereColliders.Remove(inlsc);
                        GameObject.DestroyImmediate(inlsc);
                    }
                    SortLine();
                    return true;
                }
            }
            return false;
        }
    }
}
