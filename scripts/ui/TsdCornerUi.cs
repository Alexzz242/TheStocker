using Godot;

public partial class TsdCornerUi : Control
{
	[Export] public Panel TsdPanel { get; set; }
	[Export] public Label ScanPrompt { get; set; }
	[Export] public ColorRect ScanFlash { get; set; }

	private bool _scanModeActive = false;
	private float _flashTimer = 0f;

	[Signal] public delegate void ScanModeActivatedEventHandler();

	public override void _Ready()
	{
		Visible = false;
		ScanFlash.Visible = false;
		ScanFlash.Color = new Color(0, 1, 0, 0.4f);
		ScanPrompt.Text = "SCAN";
	}

	public override void _Process(double delta)
	{
		if (_flashTimer > 0)
		{
			_flashTimer -= (float)delta;
			if (_flashTimer <= 0)
				ScanFlash.Visible = false;
		}
	}

	public void ShowForInspect()
	{
		Visible = true;
		_scanModeActive = false;
		ScanPrompt.Text = "SCAN";
		ScanFlash.Visible = false;
	}

	public void Hide()
	{
		Visible = false;
		_scanModeActive = false;
	}

	public bool IsScanModeActive() => _scanModeActive;

	public void OnTsdClicked()
	{
		if (!_scanModeActive)
		{
			_scanModeActive = true;
			ScanPrompt.Text = "AIM & CLICK";
			GD.Print("Scan mode active — click the barcode");
			EmitSignal(SignalName.ScanModeActivated);
		}
	}

	public void PlayScanSuccess()
	{
		ScanFlash.Visible = true;
		_flashTimer = 0.4f;
		ScanPrompt.Text = "SCANNED ✓";
		_scanModeActive = false;
	}
}
