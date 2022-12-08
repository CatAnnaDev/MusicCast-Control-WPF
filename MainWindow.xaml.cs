using MusicCast_Control_WPF.SaveLoadProfile;
using System.Windows.Input;

namespace MusicCast_Control_WPF;

public partial class MainWindow : Window
{
    private ConfigBuild config = new();
    private SystemConfig systemconfig = new();
    private ZoneConfig zoneconfig = new();
    private SaveLoadTemplate saveLoadTemplate = new();
    private string[] inputs = new string[50];
    private string[] sound_program = new string[50];
    private string ProfileSelected;
    private string input_list;
    private string sound_program_list;
    private string maxVol;
    private int maxBass = 12;
    private int minBass = -12;
    private int maxDialLevel = 3;
    private int minDialLevel = 0;
    private int tonetreblemax = 12;
    private int tonetreblemin = -12;
    private int tonebassmax = 12;
    private int tonebassmin = -12;

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
        ProfileCheck();
    }

    private async Task ProfileCheck()
    {
        if (Directory.Exists(Directory.GetCurrentDirectory() + "/Profile/"))
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory() + "/Profile/");
            FileInfo[] Files = d.GetFiles("*.json");
            string str = "";
            foreach (FileInfo file in Files)
            {
                SavedProfile.Items.Add(file.Name);
            }
        }
        else
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Profile/");
    }

    private void Read_input_list()
    {
        inputs = Regex.Replace(input_list, "[ \"\n\r\\[\\]\t]", "").Split(",");
        foreach (var data in inputs)
        {
            InputList.Items.Add(data);
        }

        foreach (var input in inputs)
            if (saveLoadTemplate.currentInput == input.ToLower())
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
            if (saveLoadTemplate.currentSoundProgram == input.ToLower())
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
            saveLoadTemplate.curVol = Convert.ToString(status["volume"]);
            saveLoadTemplate.currentInput = Convert.ToString(status["input"]);
            saveLoadTemplate.currentSoundProgram = Convert.ToString(status["sound_program"]);

            saveLoadTemplate.mute = (bool)status["mute"];
            saveLoadTemplate.PureDirect = (bool)status["pure_direct"];
            saveLoadTemplate.Enhancer = (bool)status["enhancer"];
            saveLoadTemplate.ExtraBass = (bool)status["extra_bass"];
            saveLoadTemplate.Adaptivedrc = (bool)status["adaptive_drc"];
            saveLoadTemplate.curDialLevel = int.Parse(Convert.ToString(status["dialogue_level"]));
            saveLoadTemplate.curBass = int.Parse(Convert.ToString(status["subwoofer_volume"]));
            saveLoadTemplate.tonetreble = int.Parse(Convert.ToString(status["tone_control"]["treble"]));
            saveLoadTemplate.tonebass = int.Parse(Convert.ToString(status["tone_control"]["bass"]));

            PureDirectbutton.Content = $"Pure Direct: {saveLoadTemplate.PureDirect}";
            EnhancerButton.Content = $"Enhancer: {saveLoadTemplate.Enhancer}";
            AdaptiveDrcButton.Content = $"Adaptive drc: {saveLoadTemplate.Adaptivedrc}";
            ExtraBassButton.Content = $"Extra Bass: {saveLoadTemplate.ExtraBass}";
            DialogueLevelLabel.Content = $"Dial: {saveLoadTemplate.curDialLevel}";
            BassLabel.Content = $"Bass: {saveLoadTemplate.curBass}";
            ToneTrebleLabel.Content = $"Treble: {saveLoadTemplate.tonetreble}";
            ToneBassLabel.Content = $"Bass: {saveLoadTemplate.tonebass}";

            SoundFormatLabel.Content = $"format: {signalinfo["audio"]["format"]}\nfs: {signalinfo["audio"]["fs"]}";


            if (saveLoadTemplate.mute)
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
        if (!saveLoadTemplate.mute)
        {
            await zoneconfig.setMute(true);
            saveLoadTemplate.mute = true;
            SoundDBLabel.Content = "muted";
        }
        else
        {
            await zoneconfig.setMute(false);
            saveLoadTemplate.mute = false;
        }
    }

    private async void VolUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (Convert.ToInt32(saveLoadTemplate.curVol) < Convert.ToInt32(maxVol))
        {
            var setVol = Convert.ToInt32(saveLoadTemplate.curVol) + 1;

            await zoneconfig.setVolume(setVol);
            saveLoadTemplate.curVol = Convert.ToString(setVol);
            saveLoadTemplate.mute = false;
        }
    }

    private async void VolDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (Convert.ToInt32(saveLoadTemplate.curVol) > 0)
        {
            var setVol = Convert.ToInt32(saveLoadTemplate.curVol) - 1;

            await zoneconfig.setVolume(setVol);
            saveLoadTemplate.curVol = Convert.ToString(setVol);
            saveLoadTemplate.mute = false;
        }
    }

    private async void InputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedInput = ((string)InputList.SelectedItem).ToLower();
        if (saveLoadTemplate.currentInput != selectedInput)
        {
            await zoneconfig.setInput(selectedInput);
            saveLoadTemplate.currentInput = selectedInput;
        }
    }

    private async void SoundProgramList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedInput = ((string)SoundProgramList.SelectedItem).ToLower();
        if (saveLoadTemplate.currentSoundProgram != selectedInput)
        {
            await zoneconfig.setSoundProgram(selectedInput);
            saveLoadTemplate.currentSoundProgram = selectedInput;
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
        if (saveLoadTemplate.curBass >= minBass || saveLoadTemplate.curBass <= maxBass)
        {
            saveLoadTemplate.curBass = saveLoadTemplate.curBass - 1;
            await zoneconfig.setSubwooferVolume(volume: saveLoadTemplate.curBass);
        }
    }

    private async void BassUp_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.curBass >= minBass || saveLoadTemplate.curBass <= maxBass)
        {
            saveLoadTemplate.curBass = saveLoadTemplate.curBass + 1;
            await zoneconfig.setSubwooferVolume(volume: saveLoadTemplate.curBass);
        }
    }

    private async void dialDown_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.curDialLevel >= minDialLevel || saveLoadTemplate.curDialLevel <= maxDialLevel)
        {
            saveLoadTemplate.curDialLevel = saveLoadTemplate.curDialLevel - 1;
            await zoneconfig.setDialogueLevel(value: saveLoadTemplate.curDialLevel);
        }
    }

    private async void DialUp_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.curDialLevel >= minDialLevel || saveLoadTemplate.curDialLevel <= maxDialLevel)
        {
            saveLoadTemplate.curDialLevel = saveLoadTemplate.curDialLevel + 1;
            await zoneconfig.setDialogueLevel(value: saveLoadTemplate.curDialLevel);
        }
    }

    private async void ToneBassDown_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.tonebass >= tonebassmin || saveLoadTemplate.tonebass <= tonebassmax)
        {
            saveLoadTemplate.tonebass = saveLoadTemplate.tonebass - 1;
            await zoneconfig.setToneControl(null, saveLoadTemplate.tonebass);
        }
    }

    private async void ToneBassUp_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.tonebass >= tonebassmin || saveLoadTemplate.tonebass <= tonebassmax)
        {
            saveLoadTemplate.tonebass = saveLoadTemplate.tonebass + 1;
            await zoneconfig.setToneControl(null, saveLoadTemplate.tonebass);
        }
    }

    private async void ToneTrebleDown_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.tonetreble >= tonetreblemin || saveLoadTemplate.tonetreble <= tonetreblemax)
        {
            saveLoadTemplate.tonetreble = saveLoadTemplate.tonetreble - 1;
            await zoneconfig.setToneControl(saveLoadTemplate.tonetreble, null);
        }
    }

    private async void ToneTrebleUp_Click(object sender, RoutedEventArgs e)
    {
        if (saveLoadTemplate.tonetreble >= tonetreblemin || saveLoadTemplate.tonetreble <= tonetreblemax)
        {
            saveLoadTemplate.tonetreble = saveLoadTemplate.tonetreble + 1;
            await zoneconfig.setToneControl(saveLoadTemplate.tonetreble, null);
        }
    }

    private async void PureDirectbutton_Click(object sender, RoutedEventArgs e)
    {
        if (!saveLoadTemplate.PureDirect)
            await zoneconfig.setPureDirect(true);
        else
            await zoneconfig.setPureDirect(false);
        PureDirectbutton.Content = $"Pure Direct: {saveLoadTemplate.PureDirect}";
    }

    private async void EnhancerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!saveLoadTemplate.Enhancer)
            await zoneconfig.setEnhancer(true);
        else
            await zoneconfig.setEnhancer(false);
        EnhancerButton.Content = $"Enhancer: {saveLoadTemplate.Enhancer}";
    }

    private async void AdaptiveDrcButton_Click(object sender, RoutedEventArgs e)
    {
        if (!saveLoadTemplate.Adaptivedrc)
            await zoneconfig.setAdaptiveDrc(true);
        else
            await zoneconfig.setAdaptiveDrc(false);
        AdaptiveDrcButton.Content = $"Adaptive drc: {saveLoadTemplate.Adaptivedrc}";
    }

    private async void ExtraBassButton_Click(object sender, RoutedEventArgs e)
    {
        if (!saveLoadTemplate.ExtraBass)
            await zoneconfig.setExtraBass(true);
        else
            await zoneconfig.setExtraBass(false);
        ExtraBassButton.Content = $"Extra Bass: {saveLoadTemplate.ExtraBass}";
    }

    private async void PureDirectbutton1_Click(object sender, RoutedEventArgs e)
    {
        // idk for now
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var Profile1 = new SaveLoadTemplate
        {
            currentInput = saveLoadTemplate.currentInput,
            currentSoundProgram = saveLoadTemplate.currentSoundProgram,
            curVol = saveLoadTemplate.curVol,
            curBass = saveLoadTemplate.curBass,
            curDialLevel = saveLoadTemplate.curDialLevel,
            tonetreble = saveLoadTemplate.tonetreble,
            tonebass = saveLoadTemplate.tonebass,
            mute = saveLoadTemplate.mute,
            PureDirect = saveLoadTemplate.PureDirect,
            Enhancer = saveLoadTemplate.Enhancer,
            ExtraBass = saveLoadTemplate.ExtraBass,
            Adaptivedrc = saveLoadTemplate.Adaptivedrc
        };

        if (ProfileSelected == null)
            ProfileSelected = "Profile1.json";

        string fileName = $"Profile/{ProfileSelected}";
        var save = JsonConvert.SerializeObject(Profile1, Formatting.Indented);
        File.WriteAllText(fileName, save);

        await ProfileCheck();
    }

    private async void Load_Click(object sender, RoutedEventArgs e)
    {
        string fileName = $"Profile/{ProfileSelected}";
        string jsonString = File.ReadAllText(fileName);
        var load = JsonConvert.DeserializeObject<SaveLoadTemplate>(jsonString);

        if(saveLoadTemplate.currentInput != load.currentInput)
            await zoneconfig.setInput(load.currentInput);
        if (saveLoadTemplate.currentSoundProgram != load.currentSoundProgram)
            await zoneconfig.setSoundProgram(load.currentSoundProgram);
        if (saveLoadTemplate.curVol != load.curVol)
            await zoneconfig.setVolume(Convert.ToInt32(load.curVol));
        if (saveLoadTemplate.curBass != load.curBass)
            await zoneconfig.setSubwooferVolume(volume: load.curBass);
        if (saveLoadTemplate.curDialLevel != load.curDialLevel)
            await zoneconfig.setDialogueLevel(value: load.curDialLevel);
        if (saveLoadTemplate.tonetreble != load.tonetreble)
            await zoneconfig.setToneControl(load.tonetreble, null);
        if (saveLoadTemplate.tonebass != load.tonebass)
            await zoneconfig.setToneControl(null, load.tonebass);
        if (saveLoadTemplate.mute != load.mute)
            await zoneconfig.setMute(load.mute);
        if (saveLoadTemplate.PureDirect != load.PureDirect)
            await zoneconfig.setPureDirect(load.PureDirect);
        if (saveLoadTemplate.Enhancer != load.Enhancer)
            await zoneconfig.setEnhancer(load.Enhancer);
        if (saveLoadTemplate.ExtraBass != load.ExtraBass)
            await zoneconfig.setExtraBass(load.ExtraBass);
        if (saveLoadTemplate.Adaptivedrc != load.Adaptivedrc)
            await zoneconfig.setAdaptiveDrc(load.Adaptivedrc);

        foreach (var input in inputs)
            if (saveLoadTemplate.currentInput == input.ToLower())
                InputList.Text = input;

        foreach (var input in sound_program)
            if (saveLoadTemplate.currentSoundProgram == input.ToLower())
                SoundProgramList.Text = input;
    }

    private void SavedProfile_SelectionChanged(object sender, SelectionChangedEventArgs e) => ProfileSelected = SavedProfile.SelectedItem.ToString();
}
