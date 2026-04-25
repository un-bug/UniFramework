using System;
using UnityEngine;

namespace UniFramework.Runtime
{
    public sealed class Entity : MonoBehaviour
    {
        public int Id { get; private set; }
        public string EntityAssetKey { get; private set; }

        public void OnInit(int entityId, string entityAssetKey)
        {
            Id = entityId;
            EntityAssetKey = entityAssetKey;
        }
    }
}