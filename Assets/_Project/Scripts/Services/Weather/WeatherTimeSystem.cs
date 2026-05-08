using System;
using LibraryGame.Services.Save;
using UnityEngine;

namespace LibraryGame.Services.Weather
{
    /// <summary>
    /// Drives the directional sun + ambient/fog tints based on local device
    /// time, and toggles a simple weather effect (clear / rain / snow).
    /// Lightweight ParticleSystem spawned at runtime.
    /// </summary>
    public sealed class WeatherTimeSystem
    {
        public const string Clear = "clear";
        public const string Rain  = "rain";
        public const string Snow  = "snow";

        private readonly SaveSystem _save;
        private Light _sun;
        private GameObject _fxRoot;
        private ParticleSystem _rain;
        private ParticleSystem _snow;
        private float _accum;

        public string CurrentWeather => _save.Player.lastWeather ?? Clear;

        public WeatherTimeSystem(SaveSystem save)
        {
            _save = save;
        }

        public void Bind(Light directionalSun, Transform fxParent)
        {
            _sun = directionalSun;
            _fxRoot = new GameObject("[WeatherFX]");
            _fxRoot.transform.SetParent(fxParent, false);

            _rain = MakeParticleSystem(_fxRoot.transform, isSnow: false);
            _snow = MakeParticleSystem(_fxRoot.transform, isSnow: true);

            // Apply persisted weather.
            SetWeather(CurrentWeather);
            ApplyTimeOfDay(CurrentLocalNormalizedTime());
        }

        /// <summary>Call from a MonoBehaviour Update.</summary>
        public void Tick(float deltaTime)
        {
            _accum += deltaTime;
            if (_accum < 1f) return;
            _accum = 0f;
            ApplyTimeOfDay(CurrentLocalNormalizedTime());
        }

        public void SetWeather(string w)
        {
            if (w != Clear && w != Rain && w != Snow) w = Clear;
            _save.Player.lastWeather = w;
            _save.SavePlayer();
            if (_rain != null) Toggle(_rain, w == Rain);
            if (_snow != null) Toggle(_snow, w == Snow);
        }

        private static void Toggle(ParticleSystem ps, bool on)
        {
            if (on) { if (!ps.isPlaying) ps.Play(); }
            else    { if (ps.isPlaying) ps.Stop(); }
        }

        // 0 = midnight, 0.5 = noon, 1 = next midnight
        private static float CurrentLocalNormalizedTime()
        {
            var now = DateTime.Now;
            return (float)((now.Hour * 3600 + now.Minute * 60 + now.Second) / 86400.0);
        }

        private void ApplyTimeOfDay(float t)
        {
            // Sun azimuth/elevation: rises at 0.25 (6am), peaks at 0.5 (noon), sets at 0.75 (6pm)
            float angle = Mathf.LerpAngle(-90f, 270f, t); // -90 at 0, 90 at 0.5, 270 at 1
            float elevation = Mathf.Sin((t - 0.25f) * Mathf.PI * 2f) * 60f; // -60 to +60

            if (_sun != null)
            {
                _sun.transform.rotation = Quaternion.Euler(elevation, 30f, 0f);
                // Color: warm at sunrise/sunset, neutral at noon, cool/dim at night.
                Color sunColor = SunColor(t);
                _sun.color = sunColor;
                _sun.intensity = SunIntensity(t);
            }

            // Ambient + fog
            RenderSettings.ambientSkyColor = AmbientSky(t);
            RenderSettings.ambientEquatorColor = Color.Lerp(AmbientSky(t), new Color(0.18f, 0.18f, 0.22f), 0.5f);
            RenderSettings.fogColor = AmbientSky(t) * 0.85f;
        }

        private static Color SunColor(float t)
        {
            // Warm at edges, cool near noon.
            if (t < 0.25f || t > 0.75f) return new Color(0.40f, 0.45f, 0.65f);   // night moonlight
            float dayT = (t - 0.25f) * 2f; // 0 at sunrise, 1 at sunset
            // 0..0.15 sunrise warm, 0.15..0.85 daylight, 0.85..1 sunset warm.
            if (dayT < 0.15f) return Color.Lerp(new Color(1.0f,0.55f,0.30f), new Color(1.0f,0.96f,0.86f), dayT/0.15f);
            if (dayT > 0.85f) return Color.Lerp(new Color(1.0f,0.96f,0.86f), new Color(1.0f,0.45f,0.25f), (dayT-0.85f)/0.15f);
            return new Color(1.0f, 0.96f, 0.86f);
        }

        private static float SunIntensity(float t)
        {
            if (t < 0.25f || t > 0.75f) return 0.20f; // night
            float dayT = (t - 0.25f) * 2f;
            return 0.45f + Mathf.Sin(dayT * Mathf.PI) * 0.65f;
        }

        private static Color AmbientSky(float t)
        {
            if (t < 0.25f || t > 0.75f) return new Color(0.10f, 0.12f, 0.20f);
            float dayT = (t - 0.25f) * 2f;
            if (dayT < 0.15f) return Color.Lerp(new Color(0.55f,0.40f,0.40f), new Color(0.55f,0.62f,0.75f), dayT/0.15f);
            if (dayT > 0.85f) return Color.Lerp(new Color(0.55f,0.62f,0.75f), new Color(0.55f,0.35f,0.32f), (dayT-0.85f)/0.15f);
            return new Color(0.55f, 0.62f, 0.75f);
        }

        private static ParticleSystem MakeParticleSystem(Transform parent, bool isSnow)
        {
            var go = new GameObject(isSnow ? "Snow" : "Rain");
            go.transform.SetParent(parent, false);
            // Position the emitter above the room so particles fall through it.
            go.transform.localPosition = new Vector3(0, RuntimeRoomHeight + 6f, 0);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = isSnow ? 6f : 1.5f;
            main.startSpeed = isSnow ? 1.0f : 12f;
            main.startSize = isSnow ? 0.06f : 0.04f;
            main.startColor = isSnow ? new Color(1f, 1f, 1f, 0.9f) : new Color(0.7f, 0.85f, 1f, 0.7f);
            main.gravityModifier = isSnow ? 0.05f : 0f; // rain handles its own straight fall
            main.maxParticles = isSnow ? 600 : 1200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = isSnow ? 100f : 350f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 0.1f, 20f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(isSnow ? -0.7f : -10f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = isSnow ? 1f : 4f;

            ps.Stop();
            return ps;
        }

        private const float RuntimeRoomHeight = 3.2f;
    }
}
