namespace MusicCast_Control_WPF;

public partial class MainWindow : Window
{
    private ConfigBuild config = new();
    private SystemConfig systemconfig = new();
    private ZoneConfig zoneconfig = new();
    private string[] inputs = new string[50];
    private string[] sound_program = new string[50];

    private string input_list;
    private string sound_program_list;
    private string currentInput;
    private string currentSoundProgram;
    private string maxVol;
    private string curVol;
    private bool mute;
    private bool PureDirect;
    private bool Enhancer;
    private bool ExtraBass;
    private bool Adaptivedrc;
    private int maxBass = 12;
    private int minBass = -12;
    private int curBass;
    private int maxDialLevel = 3;
    private int minDialLevel = 0;
    private int curDialLevel;
    private int tonetreblemax = 12;
    private int tonetreblemin = -12;
    private int tonetreble;
    private int tonebassmax = 12;
    private int tonebassmin = -12;
    private int tonebass;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await config.InitializeAsync();
        Setup();
    }

    private void Setup()
    {
        YamahaAV.ip = config.Config.IP;
        fetch_info();
    }

    private void Read_input_list()
    {
        inputs = Regex.Replace(input_list, "[ \"\n\r\\[\\]\t]", "").Split(",");
        foreach (var data in inputs)
        {
            InputList.Items.Add(data);
        }

        foreach (var input in inputs)
            if (currentInput == input.ToLower())
                InputList.Text = input;
    }

    private void Read_sound_program_list()
    {
        sound_program = Regex.Replace(sound_program_list, "[ \"\n\r\\[\\]\t]", "").Split(",");
        foreach (var data in sound_program)
        {
            SoundProgramList.Items.Add(data);
        }

        foreach (var input in sound_program)
            if (currentSoundProgram == input.ToLower())
                SoundProgramList.Text = input;
    }

    private async void fetch_info()
    {
        try
        {
            IPLabel.Content = config.Config.IP;
            var statusjson = await zoneconfig.getStatus(ZoneConfig.zone.main);
            var deviceinfojson = await systemconfig.getDeviceInfo();
            var featuresjson = await systemconfig.getFeatures();
            var signalinfojson = await zoneconfig.getSignalInfo(ZoneConfig.zone.main);

            var status = JsonNode.Parse(statusjson);
            var deviceinfo = JsonNode.Parse(deviceinfojson);
            var features = JsonNode.Parse(featuresjson);
            var signalinfo = JsonNode.Parse(signalinfojson);
            input_list = Convert.ToString(features["zone"][0]["input_list"]);
            sound_program_list = Convert.ToString(features["zone"][0]["sound_program_list"]);

            powerLabel.Content = $"Power: {(string)status["power"]} Input: {(string)status["input"]} ({(string)status["input_text"]})";

            AmpliNameLabel.Content = (string)deviceinfo["model_name"];

            maxVol = Convert.ToString(status["max_volume"]);
            curVol = Convert.ToString(status["volume"]);
            currentInput = Convert.ToString(status["input"]);
            currentSoundProgram = Convert.ToString(status["sound_program"]);

            mute = (bool)status["mute"];
            PureDirect = (bool)status["pure_direct"];
            Enhancer = (bool)status["enhancer"];
            ExtraBass = (bool)status["extra_bass"];
            Adaptivedrc = (bool)status["adaptive_drc"];
            curDialLevel = int.Parse(Convert.ToString(status["dialogue_level"]));
            curBass = int.Parse(Convert.ToString(status["subwoofer_volume"]));
            tonetreble = int.Parse(Convert.ToString(status["tone_control"]["treble"]));
            tonebass = int.Parse(Convert.ToString(status["tone_control"]["bass"]));

            PureDirectbutton.Content = $"Pure Direct: {PureDirect}";
            EnhancerButton.Content = $"Enhancer: {Enhancer}";
            AdaptiveDrcButton.Content = $"Adaptive drc: {Adaptivedrc}";
            ExtraBassButton.Content = $"Extra Bass: {ExtraBass}";
            DialogueLevelLabel.Content = $"Dial: {curDialLevel}";
            BassLabel.Content = $"Bass: {curBass}";
            ToneTrebleLabel.Content = $"Treble: {tonetreble}";
            ToneBassLabel.Content = $"Bass: {tonebass}";

            SoundFormatLabel.Content = $"format: {signalinfo["audio"]["format"]}\nfs: {signalinfo["audio"]["fs"]}";


            if (mute)
                SoundDBLabel.Content = "muted";
            else
                SoundDBLabel.Content = Convert.ToString(status["actual_volume"]["value"]) + " dB";

            if (InputList.Items.Count < 1)
            {
                Read_input_list();
            }

            if (SoundProgramList.Items.Count < 1)
            {
                Read_sound_program_list();
            }
        }

        catch (Exception ex)
        {
            AmpliNameLabel.Content = "No MusicCast";
        }
        finally
        {
            fetch_info();
        }
    }

    private async void PowerOffButton_Click(object sender, RoutedEventArgs e)
    {
        await zoneconfig.setPower();
    }

    private async void Mutebutton_Click(object sender, RoutedEventArgs e)
    {
        if (!mute)
        {
            await zoneconfig.setMute(true);
            mute = true;
            SoundDBLabel.Content = "muted";
        }
        else
        {
            await zoneconfig.setMute(false);
            mute = false;
        }
    }

    private async void VolUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (Convert.ToInt32(curVol) < Convert.ToInt32(maxVol))
        {
            var setVol = Convert.ToInt32(curVol) + 1;

            await zoneconfig.setVolume(setVol);
            curVol = Convert.ToString(setVol);
            mute = false;
        }
    }

    private async void VolDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (Convert.ToInt32(curVol) > 0)
        {
            var setVol = Convert.ToInt32(curVol) - 1;

            await zoneconfig.setVolume(setVol);
            curVol = Convert.ToString(setVol);
            mute = false;
        }
    }

    private async void InputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedInput = ((string)InputList.SelectedItem).ToLower();
        if (currentInput != selectedInput)
        {
            await zoneconfig.setInput(selectedInput);
            currentInput = selectedInput;
        }
    }

    private async void SoundProgramList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedInput = ((string)SoundProgramList.SelectedItem).ToLower();
        if (currentSoundProgram != selectedInput)
        {
            await zoneconfig.setSoundProgram(selectedInput);
            currentSoundProgram = selectedInput;
        }
    }

    private void AmpliNameLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (AmpliNameLabel.Content != "Not connected")
            MessageBox.Show(
                AmpliNameLabel.Content + "\nList of supported inputs\n" + Regex.Replace(input_list, "[\\[\\]]", "").Replace("\"", "").Replace(",", ""), "Input list");
    }

    private async void BassDown_Click(object sender, RoutedEventArgs e)
    {
        if (curBass >= minBass || curBass <= maxBass)
        {
            curBass = curBass - 1;
            await zoneconfig.setSubwooferVolume(volume: curBass);
        }
    }

    private async void BassUp_Click(object sender, RoutedEventArgs e)
    {
        if (curBass >= minBass || curBass <= maxBass)
        {
            curBass = curBass + 1;
            await zoneconfig.setSubwooferVolume(volume: curBass);
        }
    }

    private async void dialDown_Click(object sender, RoutedEventArgs e)
    {
        if (curDialLevel >= minDialLevel || curDialLevel <= maxDialLevel)
        {
            curDialLevel = curDialLevel - 1;
            await zoneconfig.setDialogueLevel(value: curDialLevel);
        }
    }

    private async void DialUp_Click(object sender, RoutedEventArgs e)
    {
        if (curDialLevel >= minDialLevel || curDialLevel <= maxDialLevel)
        {
            curDialLevel = curDialLevel + 1;
            await zoneconfig.setDialogueLevel(value: curDialLevel);
        }
    }

    private async void ToneBassDown_Click(object sender, RoutedEventArgs e)
    {
        if (tonebass >= tonebassmin || tonebass <= tonebassmax)
        {
            tonebass = tonebass - 1;
            await zoneconfig.setToneControl(null, tonebass);
        }
    }

    private async void ToneBassUp_Click(object sender, RoutedEventArgs e)
    {
        if (tonebass >= tonebassmin || tonebass <= tonebassmax)
        {
            tonebass = tonebass + 1;
            await zoneconfig.setToneControl(null, tonebass);
        }
    }

    private async void ToneTrebleDown_Click(object sender, RoutedEventArgs e)
    {
        if (tonetreble >= tonetreblemin || tonetreble <= tonetreblemax)
        {
            tonetreble = tonetreble - 1;
            await zoneconfig.setToneControl(tonetreble, null);
        }
    }

    private async void ToneTrebleUp_Click(object sender, RoutedEventArgs e)
    {
        if (tonetreble >= tonetreblemin || tonetreble <= tonetreblemax)
        {
            tonetreble = tonetreble + 1;
            await zoneconfig.setToneControl(tonetreble, null);
        }
    }

    private async void PureDirectbutton_Click(object sender, RoutedEventArgs e)
    {
        if (!PureDirect)
            await zoneconfig.setPureDirect(true);
        else
            await zoneconfig.setPureDirect(false);
        PureDirectbutton.Content = $"Pure Direct: {PureDirect}";
    }

    private async void EnhancerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Enhancer)
            await zoneconfig.setEnhancer(true);
        else
            await zoneconfig.setEnhancer(false);
        EnhancerButton.Content = $"Enhancer: {Enhancer}";
    }

    private async void AdaptiveDrcButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Adaptivedrc)
            await zoneconfig.setAdaptiveDrc(true);
        else
            await zoneconfig.setAdaptiveDrc(false);
        AdaptiveDrcButton.Content = $"Adaptive drc: {Adaptivedrc}";
    }

    private async void ExtraBassButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ExtraBass)
            await zoneconfig.setExtraBass(true);
        else
            await zoneconfig.setExtraBass(false);
        ExtraBassButton.Content = $"Extra Bass: {ExtraBass}";
    }

    private async void PureDirectbutton1_Click(object sender, RoutedEventArgs e)
    {

    }
}
