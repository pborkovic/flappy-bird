using Godot;
using Flappy_Bird.Scripts.Utils.Enums;

namespace Flappy_Bird.Scripts.Controllers;

public partial class WeatherController : Node
{
    [Export] public NodePath NightOverlayPath;
    [Export] public NodePath RainParticlesPath;
    [Export] public NodePath FogOverlayPath;
    private const float WeatherChangeInterval = 30.0f;
    private const float TransitionDuration = 2.0f;
    private ColorRect _nightOverlay;
    private GpuParticles2D _rainParticles;
    private ColorRect _fogOverlay;
    private WeatherType _currentWeather = WeatherType.Clear;
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    private float _weatherTimer = 0.0f;
    private bool _autoChangeWeather = true;
    private Tween _currentTween;

    public override void _Ready()
    {
        if (NightOverlayPath != null)
            _nightOverlay = GetNodeOrNull<ColorRect>(NightOverlayPath);
        if (RainParticlesPath != null)
            _rainParticles = GetNodeOrNull<GpuParticles2D>(RainParticlesPath);
        if (FogOverlayPath != null)
            _fogOverlay = GetNodeOrNull<ColorRect>(FogOverlayPath);

        InitializeOverlays();
    }

    public override void _Process(double delta)
    {
        if (!_autoChangeWeather)
            return;

        _weatherTimer += (float)delta;

        if (_weatherTimer >= WeatherChangeInterval)
        {
            _weatherTimer = 0.0f;
            RandomizeWeather();
        }
    }

    private void InitializeOverlays()
    {
        if (_nightOverlay != null)
        {
            _nightOverlay.Color = new Color(0, 0, 0.1f, 0);
            _nightOverlay.Visible = true;
        }

        if (_fogOverlay != null)
        {
            _fogOverlay.Color = new Color(0.8f, 0.8f, 0.85f, 0);
            _fogOverlay.Visible = true;
        }

        if (_rainParticles != null)
        {
            _rainParticles.Emitting = false;
        }
    }

    public void SetWeather(WeatherType weather)
    {
        if (_currentWeather == weather)
            return;

        _currentWeather = weather;
        ApplyWeatherEffects();
    }

    public void SetTimeOfDay(TimeOfDay timeOfDay)
    {
        if (_currentTimeOfDay == timeOfDay)
            return;

        _currentTimeOfDay = timeOfDay;
        ApplyTimeOfDayEffects();
    }

    public void RandomizeWeather()
    {
        var random = new RandomNumberGenerator();
        random.Randomize();

        var weatherValues = System.Enum.GetValues<WeatherType>();
        var timeValues = System.Enum.GetValues<TimeOfDay>();

        var newWeather = weatherValues[random.RandiRange(0, weatherValues.Length - 1)];
        var newTime = timeValues[random.RandiRange(0, timeValues.Length - 1)];

        SetWeather(newWeather);
        SetTimeOfDay(newTime);
    }

    private void ApplyWeatherEffects()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);

        switch (_currentWeather)
        {
            case WeatherType.Clear:
                if (_fogOverlay != null)
                    _currentTween.TweenProperty(_fogOverlay, "color:a", 0.0f, TransitionDuration);
                if (_rainParticles != null)
                    _rainParticles.Emitting = false;
                break;

            case WeatherType.Rain:
                if (_fogOverlay != null)
                    _currentTween.TweenProperty(_fogOverlay, "color:a", 0.0f, TransitionDuration);
                if (_rainParticles != null)
                    _rainParticles.Emitting = true;
                break;

            case WeatherType.Fog:
                if (_fogOverlay != null)
                    _currentTween.TweenProperty(_fogOverlay, "color:a", 0.5f, TransitionDuration);
                if (_rainParticles != null)
                    _rainParticles.Emitting = false;
                break;
        }
    }

    private void ApplyTimeOfDayEffects()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();

        switch (_currentTimeOfDay)
        {
            case TimeOfDay.Day:
                if (_nightOverlay != null)
                    _currentTween.TweenProperty(_nightOverlay, "color:a", 0.0f, TransitionDuration);
                break;

            case TimeOfDay.Night:
                if (_nightOverlay != null)
                    _currentTween.TweenProperty(_nightOverlay, "color:a", 0.6f, TransitionDuration);
                break;
        }
    }

    public void SetAutoChangeWeather(bool enabled)
    {
        _autoChangeWeather = enabled;
    }

    public void ResetWeather()
    {
        _weatherTimer = 0.0f;
        SetWeather(WeatherType.Clear);
        SetTimeOfDay(TimeOfDay.Day);
    }

    public WeatherType GetCurrentWeather() => _currentWeather;

    public TimeOfDay GetCurrentTimeOfDay() => _currentTimeOfDay;
}
