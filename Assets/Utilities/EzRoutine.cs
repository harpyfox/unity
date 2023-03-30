using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace harpyfox.Routines
{
    public static class EzRoutine
    {
        /// <summary>
        /// Starts a Coroutine. Stops the routine first if it is already running.
        /// </summary>
        public static Coroutine StartEzRoutine(this MonoBehaviour owner, ref Coroutine reference, IEnumerator routine) {
            if (reference != null) owner.StopCoroutine(reference);
            reference = owner.StartCoroutine(routine);
            return reference;
        }
    }
}
