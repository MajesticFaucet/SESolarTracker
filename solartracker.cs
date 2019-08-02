// [----  lightweight sun-tracking solar panel script by MajesticFaucet.  ----]

const float _upposition = 270F;
const float _lowerlimit = 185F;
const float _upperlimit = 355F;
const string _rotorgroupname = "Rotors";
const float _velocitylimit = 0.5F;

// don't touch past this line
const UpdateFrequency defaultfreq = UpdateFrequency.Update100;

List<IMyMotorStator> rotors;
IMyPowerProducer panelminusx;
IMyPowerProducer panelplusx;
IMyPowerProducer panely;

// SETUP

public Program()
 {
    try {
        Setup();
    } catch(Exception e) {
        Echo("Error: " + e.Message);
    }
}

private void Setup() {
    var rotorgroup = GridTerminalSystem.GetBlockGroupWithName(_rotorgroupname);
    if(rotorgroup == null) throw new Exception("Rotor group " + _rotorgroupname + " does not exist.");
    rotors = new List<IMyMotorStator>();
    rotorgroup.GetBlocksOfType<IMyMotorStator>(rotors);
    if(rotors.Count == 0) throw new Exception("No rotors in rotor group " + _rotorgroupname + ".");

    panelminusx = GetPanel("Solar Panel [-X]");
    panelplusx = GetPanel("Solar Panel [+X]");
    panely = GetPanel("Solar Panel [Y]");

    // lower limit should never be changed so if you mess it up, not my problem (shrug).
    if(rotors[0].LowerLimitDeg != _lowerlimit) {
        foreach(var rotor in rotors) {
            rotor.LowerLimitDeg = _lowerlimit;
        }
    }
    Runtime.UpdateFrequency = defaultfreq;
}

private IMyPowerProducer GetPanel(string panelname) {
    if(string.IsNullOrEmpty(panelname)) throw new Exception("Panel name must not be empty or null.");
    var block = GridTerminalSystem.GetBlockWithName(panelname);
    if(block == null) throw new Exception("Panel " + panelname + " does not exist.");
    if(!(block is IMyPowerProducer)) throw new Exception("Block " + panelname + " is not a solar panel.");
    return (IMyPowerProducer) block;
}

// INSTANCE

public void Main(string argument, UpdateType updateSource)
{
    try {
        Run();
        if(Runtime.UpdateFrequency == UpdateFrequency.None) {
            Runtime.UpdateFrequency = defaultfreq;
        }
    } catch(Exception e) {
        Echo("Error: " + e.Message);
        Runtime.UpdateFrequency = UpdateFrequency.None;
    }
}

public void Run() {
    float y = panely.MaxOutput;
    if(y == 0F) {
        EnsureRotorParams(false, _upperlimit);
        Echo("No sun. :(");
        return;
    }
    float negx = panelminusx.MaxOutput;
    float plusx = panelplusx.MaxOutput;

    float sunangle = RadiansToDegrees((float)Math.Atan((plusx-negx)/y));
    float tmppx = _upposition+sunangle;
    EnsureRotorParams(true, tmppx<=_upperlimit ? tmppx : _upperlimit);
    Echo("Sun angle: " + sunangle.ToString());
}

private void EnsureRotorParams(bool vel, float up) {
    float fvel = vel ? _velocitylimit : _velocitylimit * -1F;
    if(rotors[0].TargetVelocityRPM != fvel) {
        foreach(var rotor in rotors) {
            rotor.TargetVelocityRPM = fvel;
        }
    }
    // BUG: UpperLimitDeg accepts radians for setter.
    up = DegreesToRadians(up);
    if(rotors[0].UpperLimitRad != up) {
        foreach(var rotor in rotors) {
            rotor.UpperLimitRad = up;
        }
    }
}

private float RadiansToDegrees(float val) {
    return val*(180F/(float)Math.PI);
}

private float DegreesToRadians(float val) {
    return val/(180F/(float)Math.PI);
}