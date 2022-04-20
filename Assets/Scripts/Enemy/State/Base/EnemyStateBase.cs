namespace Enemy.State
{
    public abstract class EnemyStateBase
    {
        public abstract void InitState(Enemy enemy);
        public abstract void UpdateState(Enemy enemy);
        public abstract void EndState(Enemy enemy);
    }
}