// Separate helper file.
public partial class Controller : IController
{
    // (A bunch of helper methods on Controller)
    // ...
    RequestContext IController.RequestContext { get; set; }
}