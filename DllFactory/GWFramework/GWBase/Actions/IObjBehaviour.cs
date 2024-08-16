
using System.Xml.Linq;
using System.Xml.Serialization;


public interface IObjBehaviour {
    public void Start(XElement possess, object[] parameters);
    public void Tick(object[] parameters, float deltaTime);
    public void RareTick(object[] parameters, float deltaTime);
    public void Suspend(object[] parameters);
    public string GetName();
    public ParameterRequest[] GetParameters();

}