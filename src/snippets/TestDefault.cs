public interface IController
{
    void ProcessRequest(bool useRetries = true);
}

public partial class Controller : IController
{
    public void TestDefault()
    {
        this.ProcessRequest(); // Validate: False
        ((IController)this).ProcessRequest(); // Validate: True
    }

    public void ProcessRequest(bool validate = false)
    {
        Console.WriteLine("Validate: " + validate);
    }
}