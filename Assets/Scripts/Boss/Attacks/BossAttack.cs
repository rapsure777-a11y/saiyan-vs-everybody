using System.Collections;

namespace Saiyan.Boss
{
    /// <summary>
    /// One King Cakezilla attack. Telegraph shows the warning (markers, color, sound) and must last at least <see cref="BossTuning.MinTelegraphSeconds"/>;
    /// Execute is when damage can happen. Attacks are coroutines driven by <see cref="BossController"/>.
    /// </summary>
    public abstract class BossAttack
    {
        public abstract string Name { get; }
        public abstract float TelegraphSeconds(BossContext c);
        public abstract float Weight(BossContext c);
        public abstract IEnumerator Telegraph(BossContext c);
        public abstract IEnumerator Execute(BossContext c);
    }
}
