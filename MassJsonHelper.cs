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
	public int TrailSeconds {get; set;}
    public double TrailQuality {get; set;}

    public int FollowingIndex {get; set;}
	public int OrbitingBodyIndex {get; set;}

    public bool Satellite {get; set;}
    public double SemiMajorAxisLength {get; set;}
    public int PrecisionPriorityLimitInSeconds {get; set;}

	internal static void SaveMassesToFile(List<Mass> masses, string fileName) {
		MassJsonList JList = new();
		foreach(Mass m in masses) {
			JList.JsonMasses.Add(new MassJsonHelper(m.mi));
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
			jsonMasses = JsonSerializer.Deserialize<MassJsonList>(file.ReadToEnd());
		}
		if(jsonMasses == null) { return null; }
		
		foreach(MassJsonHelper h in jsonMasses.JsonMasses) {
			masses.Add(h.ToMass());
		}
		return masses;
	}
	internal Mass ToMass() {
		return new Mass(MassInGg, VelocityInKmS, PositionInKm, name: Name, hasTrail: HasTrail, stationary: Stationary, 
			trailSeconds: TrailSeconds, trailQuality: TrailQuality, followingIndex: FollowingIndex, 
			precisionPriorityLimit: PrecisionPriorityLimitInSeconds, satellite: Satellite);
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
		TrailSeconds = (int) (mass.trail.Length / mass.trailQuality);
		TrailQuality = mass.trailQuality;

		FollowingIndex = mass.followingIndex;
		OrbitingBodyIndex = mass.orbitingBodyIndex;

		Satellite = mass.satellite;
		SemiMajorAxisLength = mass.semiMajorAxisLength;
		PrecisionPriorityLimitInSeconds = mass.precisionPriorityLimit;
	}
	[JsonConstructor]
	public MassJsonHelper(){
		Name = "";
	}
}
public class MassJsonList {
	public List<MassJsonHelper> JsonMasses {get; set;}
	public MassJsonList() {
		JsonMasses = new();
	}
}