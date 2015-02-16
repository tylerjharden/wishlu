using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class NotificationsController : BaseController
    {        
        public ActionResult Index()
        {
            List<Squid.Messages.Notification> model = Squid.Users.User.GetNotifications(GetCurrentUserId());
            
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(DeleteNotificationModel model)
        {
            try
            {
                Squid.Messages.Notification n = Squid.Messages.Notification.GetNotificationById(model.Id);

                if (n.UserId != GetCurrentUserId())
                    return Json(false);

                n.DeleteAll();

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult MarkAsRead(MarkNotificationModel model)
        {
            try
            {
                Squid.Messages.Notification n = Squid.Messages.Notification.GetNotificationById(model.Id);

                if (n.UserId != GetCurrentUserId())
                    return Json(false);

                n.MarkAsRead();

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }
    }

    public class DeleteNotificationModel
    {
        public Guid Id { get; set; }
    }

    public class MarkNotificationModel
    {
        public Guid Id { get; set; }
    }
}