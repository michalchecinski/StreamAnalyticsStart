using EventReceiverWebApp.Models;
using EventReceiverWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventReceiverWebApp.Controllers
{
    public class Events : Controller
    {
        public AtmEvent[] GetEvents()
        {
            return AtmEventAggregator.GetLoggedEvents();
        }
    }
}