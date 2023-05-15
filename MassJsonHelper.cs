using Gtk;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbit;
public class MassJsonHelper {
	public int Index {get; set;}
	public string Name {get; set;}
	public bool MinorMass {get; set;}

	public double MassInGg {get; set;}
	public Vector2d PositionInKm {get; set;}
	public Vector2d VelocityInKmS {get; set;}
	public bool Stationary {get; set;}

    public bool HasTrail {get; set;}
	public int TrailLength {get; set;}
    public double TrailQuality {get; set;}

    public int FollowingIndex {get; set;}
	public int OrbitingBodyIndex {get; set;}

    public bool Satellite {get; set;}
    public double SemiMajorAxisLength {get; set;}
    public int PrecisionPriorityLimitInSeconds {get; set;}

	internal static void SaveMassesToFile(List<Mass> masses, string fileName) {
		MassJsonList JList = new();
		foreach(Mass m in masses) {
			JList.Masses.Add(new MassJsonHelper(m.mi));
		}
		switch(Shared.resolutionMode) {
			case 1:
				JList.ResolutionMode = "Low";
				break;
			case 2:
				JList.ResolutionMode = "Medium";
				break;
			case 3:
				JList.ResolutionMode = "High";
				break;
			default:
				JList.ResolutionMode = "None";
				break;
		}
		var options = new JsonSerializerOptions { WriteIndented = true };
		string jsonString = JsonSerializer.Serialize(JList, options);
		
		if(fileName.EndsWith(".json")) {
			File.WriteAllText(@fileName, jsonString);
		}
		else {
			File.WriteAllText(@fileName + ".json", jsonString);
		}
	}
	internal static List<Mass>? LoadMassesFromFile(string fileName) {
		MassJsonList? jsonMasses;
		List<Mass> masses = new();
		using (StreamReader file = File.OpenText(@fileName)) {
			try {
				jsonMasses = JsonSerializer.Deserialize<MassJsonList>(file.ReadToEnd());
			}
			catch {
				return null;
			}
		}
		if(jsonMasses == null) { return null; }
		
		foreach(MassJsonHelper h in jsonMasses.Masses) {
			masses.Add(h.ToMass());
		}
		switch(jsonMasses.ResolutionMode) {
			case "Low":
				Shared.resolutionMode = 1;
				break;
			case "Medium":
				Shared.resolutionMode = 2;
				break;
			case "High":
				Shared.resolutionMode = 3;
				break;
			default:
				Shared.resolutionMode = 0;
				break;
		}
		return masses;
	}
	internal Mass ToMass() {
		Mass m = new Mass(MassInGg, VelocityInKmS, PositionInKm, name: Name, hasTrail: HasTrail, stationary: Stationary, 
			trailLength: TrailLength, trailQuality: TrailQuality, followingIndex: FollowingIndex, 
			precisionPriorityLimit: PrecisionPriorityLimitInSeconds, satellite: Satellite, orbitingBodyIndex: OrbitingBodyIndex);

		m.mi.minorMass = MinorMass;
		return m;
	}
	internal MassJsonHelper(MassInfo mass) : base(){
		Index = mass.index;
		Name = mass.name;
		MinorMass = mass.minorMass;

		MassInGg = mass.mass;
		PositionInKm = mass.position;
		VelocityInKmS = mass.velocity;
		Stationary = mass.stationary;

		HasTrail = mass.hasTrail;
		TrailLength = mass.trail.Length / 100;
		TrailQuality = 8 - Math.Log10(mass.trailQuality / 60);

		FollowingIndex = mass.followingIndex;
		OrbitingBodyIndex = mass.orbitingBodyIndex;

		Satellite = mass.satellite;
		PrecisionPriorityLimitInSeconds = mass.precisionPriorityLimit;
	}
	[JsonConstructor]
	public MassJsonHelper(){
		Name = "";
	}
}
public class MassJsonList {
	public String ResolutionMode {get; set;}
	public List<MassJsonHelper> Masses {get; set;}
	public MassJsonList() {
		ResolutionMode = "None";
		Masses = new();
	}
}