﻿// Controllers/RootController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")]
public class RootController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Backend is running.");
}
