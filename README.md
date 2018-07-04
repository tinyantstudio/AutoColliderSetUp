# AutoColliderSetUp
Tools set up Character's colliders We use Humanoid Character to get Avatar bone Define. Support Unity's Normal Collider and Fake LineSphere "Collider".

### How to use

![Window](https://github.com/frideal/AutoColliderSetUp/blob/master/Files/window.png)

##### Auto Wrap
1. Open FTP_Tools/ FTP - AutoWrapHumanBodyColliders.
2. Select Collider Type (Normal and Line-Sphere Fake Collider).
3. Check if target is Humanoid and avatar is set.
4. **Normal Type : click Mapping Bone and Auto create collider**


5. Fake Collider Type : click Mapping Bone
6. Fake Collider Type : click reset and change bone factors to finish the init config
7. Fake Collider Type : after finish init you can judge each bone collider with handler in the scene
8. Fake Collider Type : add or delete bone in the inspector you can see handler(Clone) in the scene

##### Clear and Copy Collider
1. Open FTP_Tools/FTP - ColliderTools
2. Select src and des target
3. If they are same hierarchy just copy with name
4. If they are unsame bone hierarchy u can use Avatar to map the right bone
5. Select target you want to clear and click clear (normal collider and fake line-sphere collider)


#### Result
![Shot](https://github.com/frideal/AutoColliderSetUp/blob/master/Files/shot.png)