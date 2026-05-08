using LibraryGame.Core;
using LibraryGame.Services.Weather;
using UnityEngine;

namespace LibraryGame.Gameplay
{
    public sealed class WeatherTicker : MonoBehaviour
    {
        private WeatherTimeSystem _ws;

        private void Start() { ServiceLocator.TryGet(out _ws); }
        private void Update() { _ws?.Tick(Time.deltaTime); }
    }
}
