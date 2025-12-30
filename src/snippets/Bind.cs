public IController CreateController(HttpRequestMessage request)
{
    IController controller = new Controller();
    controller.RequestContext = new RequestContext(request);
    // ...
    return controller;
}