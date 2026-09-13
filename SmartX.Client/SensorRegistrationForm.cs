using SmartX.Shared;

namespace SmartX.Client;

public class SensorRegistrationForm : Form
{
    private readonly ApiClient _apiClient;
    private TextBox _macTextBox = null!;
    private TextBox _locationTextBox = null!;
    private ComboBox _categoryComboBox = null!;
    private Label _statusLabel = null!;
    private string? _pendingAttachmentPath;

    public SensorRegistrationForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Register Sensor";
        Width = 420;
        Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var macLabel = new Label { Text = "Device MAC Address", Location = new Point(20, 20), AutoSize = true };
        _macTextBox = new TextBox { Location = new Point(20, 45), Width = 360 };

        var locationLabel = new Label
        {
            Text = "Deployment Location, e.g. Facility A > Zone 1 > Sub-Zone B",
            Location = new Point(20, 80),
            AutoSize = true
        };
        _locationTextBox = new TextBox { Location = new Point(20, 105), Width = 360 };

        var categoryLabel = new Label { Text = "Sensor Category", Location = new Point(20, 140), AutoSize = true };
        _categoryComboBox = new ComboBox
        {
            Location = new Point(20, 165),
            Width = 360,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _categoryComboBox.Items.AddRange(Enum.GetNames(typeof(SensorCategory)));
        _categoryComboBox.SelectedIndex = 0;

        var attachButton = new Button { Text = "Attach Config / Photo / Log", Location = new Point(20, 200), Width = 200 };
        attachButton.Click += AttachButton_Click;

        var registerButton = new Button { Text = "Register Sensor", Location = new Point(240, 200), Width = 140 };
        registerButton.Click += RegisterButton_Click;

        _statusLabel = new Label
        {
            Location = new Point(20, 240),
            Width = 360,
            Height = 40,
            ForeColor = Color.DarkRed
        };

        Controls.Add(macLabel);
        Controls.Add(_macTextBox);
        Controls.Add(locationLabel);
        Controls.Add(_locationTextBox);
        Controls.Add(categoryLabel);
        Controls.Add(_categoryComboBox);
        Controls.Add(attachButton);
        Controls.Add(registerButton);
        Controls.Add(_statusLabel);
    }

    private void AttachButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*",
            Title = "Select a config file, photo or log to attach"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _pendingAttachmentPath = dialog.FileName;
            _statusLabel.ForeColor = Color.Black;
            _statusLabel.Text = $"Attached: {Path.GetFileName(_pendingAttachmentPath)}";
        }
    }

    private async void RegisterButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_macTextBox.Text) || string.IsNullOrWhiteSpace(_locationTextBox.Text))
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Mac address and location cant be empty";
            return;
        }

        // recursion check before we even bother the api, catches a badly
        // typed location path like a stray > with nothing after it
        var node = BuildLocationTree(_locationTextBox.Text);
        if (!DeploymentValidator.ValidateNode(node))
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Location path is not a valid deployment config";
            return;
        }

        var request = new ApiClient.RegisterSensorRequest
        {
            MacAddress = _macTextBox.Text,
            Location = _locationTextBox.Text,
            Category = Enum.Parse<SensorCategory>((string)_categoryComboBox.SelectedItem!)
        };

        var sensor = await _apiClient.RegisterSensorAsync(request);
        if (sensor == null)
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Could not register sensor, check the api is running";
            return;
        }

        if (_pendingAttachmentPath != null)
        {
            await _apiClient.UploadAttachmentAsync(sensor.Id, _pendingAttachmentPath);
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    // splits "Facility A > Zone 1 > Sub-Zone B" into a small tree so it can
    // be checked recursively instead of just trusting the raw string
    private static DeploymentNode BuildLocationTree(string location)
    {
        var parts = location.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return new DeploymentNode(location.Trim());
        }

        var root = new DeploymentNode(parts[0]);
        var current = root;
        for (int i = 1; i < parts.Length; i++)
        {
            var child = new DeploymentNode(parts[i]);
            current.Children.Add(child);
            current = child;
        }
        return root;
    }
}
