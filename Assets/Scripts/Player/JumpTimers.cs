using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>
    /// Coyote time (a jump still works shortly after leaving a ledge) and jump buffer (a press shortly before landing still jumps). Pure logic, unit tested.
    /// </summary>
    public sealed class JumpTimers
    {
        public float Coyote = 0.1f, Buffer = 0.12f;
        float m_Coyote, m_Buffer;

        public float CoyoteLeft => m_Coyote;
        public float BufferLeft => m_Buffer;

        public void Tick(float dt, bool grounded, bool pressedThisFrame)
        {
            m_Coyote = grounded ? Coyote : Mathf.Max(0f, m_Coyote - dt);
            m_Buffer = pressedThisFrame ? Buffer : Mathf.Max(0f, m_Buffer - dt);
        }

        /// <summary>True (once) when a buffered press can be spent on a jump now.</summary>
        public bool TryConsume()
        {
            if (m_Buffer > 0f && m_Coyote > 0f) { m_Buffer = 0f; m_Coyote = 0f; return true; }
            return false;
        }

        public void Reset() { m_Coyote = 0f; m_Buffer = 0f; }
    }
}
