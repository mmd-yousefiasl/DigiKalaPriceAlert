namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaSessionState
    {
        private volatile bool _warmedUp = false;
        private DateTime _warmedUpAt = DateTime.MinValue;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly TimeSpan _sessionLifetime = TimeSpan.FromMinutes(20);

        public bool IsWarmedUp =>
            _warmedUp && (DateTime.UtcNow - _warmedUpAt) < _sessionLifetime;

        public SemaphoreSlim Lock => _lock;

        public void MarkWarmedUp()
        {
            _warmedUpAt = DateTime.UtcNow;
            _warmedUp = true;
        }

        public void Reset()
        {
            _warmedUp = false;
            _warmedUpAt = DateTime.MinValue;
        }
    }
}
