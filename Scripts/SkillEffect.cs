﻿// Base component for skill effects.
//
// About Server/Client simulation:
//   There is a useful optimization that we can do to save lots of bandwidth:
//   By default, we always do all the logic on the server and then just synchro-
//   nize the position to the client via NetworkTransform. This is perfectly
//   fine and you should do that to be save.
//
//   It's important to know that most effects can be done without any synchroni-
//   zations, saving lots of bandwidth. For example:
//   - An arrow just flies to the target with some speed. We can do that on the
//     client and it will be the same result as on the server.
//   - Even a lightning strike that jumps to other entities can be done without
//     any NetworkTransform if we assume that it always jumps to the closest
//     entity. That will be the same on the server and on the client.
//
//   In other words: use 'if (isServer)' to simulate all the logic and use
//   NetworkTransform to synchronize it to clients. Buf if you are an expert,
//   you might as well avoid NetworkTransform and simulate on server and client.
//
// Note: make sure to drag all your SkillEffect prefabs into the NetworkManager
//   spawnable prefabs list.
using UnityEngine;

public abstract class SkillEffect : MonoBehaviour
{
  [ HideInInspector] public Entity target;
  [ HideInInspector] public Entity caster;
}
