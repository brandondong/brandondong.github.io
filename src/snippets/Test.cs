public partial class Controller : IController
{
    public void Test()
    {
        Test1(this); // True
        Test2(this); // False
        Test3(); // False
    }

    public void Test1(IController controller)
    {
        Console.WriteLine(controller.RequestContext != null);
    }

    public void Test2(Controller controller)
    {
        Console.WriteLine(controller.RequestContext != null);
    }

    public void Test3()
    {
        Console.WriteLine(this.RequestContext != null);
    }
}