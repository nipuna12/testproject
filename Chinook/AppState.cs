namespace Chinook
{
    public class AppState
    {
        public event Action OnChange;

        public void UpdatePlaylistStore()
        {
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
