using Godot;

public partial class Flashlight : SpotLight3D
{
	[Export] public float BlinkDuration { get; set; } = 5.0f;
	[Export] public float CooldownDuration { get; set; } = 3.0f;
	[Export] public float BlinkStartAt { get; set; } = 3.5f; // starts blinking after this many seconds
	[Export] public float BlinkSpeed { get; set; } = 8.0f;

	private enum FlashlightState { Off, On, Blinking, Cooldown }
	private FlashlightState _state = FlashlightState.Off;

	private float _timer = 0f;
	private float _blinkTimer = 0f;

	public override void _Ready()
	{
		Visible = false;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		switch (_state)
		{
			case FlashlightState.Off:
				Visible = false;
				break;

			case FlashlightState.On:
				Visible = true;
				_timer += dt;

				// Start blinking when timer hits threshold
				if (_timer >= BlinkStartAt)
					_state = FlashlightState.Blinking;
				break;

			case FlashlightState.Blinking:
				_timer += dt;
				_blinkTimer += dt;

				// Blink by toggling visibility rapidly
				Visible = Mathf.Sin(_blinkTimer * BlinkSpeed) > 0;

				// After full duration, go to cooldown
				if (_timer >= BlinkDuration)
				{
					Visible = false;
					_state = FlashlightState.Cooldown;
					_timer = 0f;
					_blinkTimer = 0f;
				}
				break;

			case FlashlightState.Cooldown:
				Visible = false;
				_timer += dt;

				if (_timer >= CooldownDuration)
				{
					_timer = 0f;
					_state = FlashlightState.Off;
				}
				break;
		}
	}

	public void Toggle()
	{
		// Only allow toggle when Off
		if (_state == FlashlightState.Off)
		{
			_timer = 0f;
			_blinkTimer = 0f;
			_state = FlashlightState.On;
		}
	}

	public bool IsAvailable() => _state == FlashlightState.Off;
}
