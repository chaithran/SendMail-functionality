   public int SendEmailWithAttachments(int litigationId, string Users, string actionBy)
    {
        int Res = 0;
        string directoryInfo = ConfigurationManager.AppSettings["EmailAttachments"];
        var directory = new DirectoryInfo(directoryInfo);
        string dynamicURL = WebConfigurationManager.AppSettings["AppUrl"];
        string Parameters = "Entering SendEmailWithAttachments";

        var FileInDirectory = directory.GetFiles()
                     .OrderByDescending(f => f.LastWriteTime)
                     .First();

        string attachmentPath = FileInDirectory.FullName;

        string[] UserEmails = Users.Split(';');
        for (int i = 0; i < UserEmails.Length; i++)
        {
            if ("@" + UserEmails[i].Split('@')[1] != ConfigurationManager.AppSettings["DomainEmail"].ToString())
            {
                Res = -1;
            }

        }
        if (Res != -1)
        {
            var finalList = (from m in dbEntities.usp_getNotificationGrid()
                             select new clsEmailTemplateVM
                             {
                                 NotificationId = m.NotificationID,
                                 Module = m.Module,
                                 ActionType = m.Action,
                                 Subject = m.Subject,
                                 EmailBody = m.Body
                             }).ToList();
            var email = finalList.Where(m => m.Module == "SampleModule" && m.ActionType == "Email").FirstOrDefault();
            string userId = dbEntities.tbl_Users.Select(m => m.CreatedBy).SingleOrDefault();
            string Sender = dbEntities.tbl_Resource.Where(x => x.UserID == userId).Select(x => x.Name).SingleOrDefault();

            var Body = email != null ? email.EmailBody : "";
            Body = String.Format(Body.Replace("@url", "" + @dynamicURL + ""));
            Body = Body.Replace("@Sender", "" + Sender + "");
            try
            {
                using (var dbEntities = new RMS_MVCEntities())
                {
                    string Parameters_two = "Before SmtpClient";

                    using (var client = new SmtpClient())
                    {
                        try
                        {
                            MailMessage mail = new MailMessage();
                            string sender = ConfigurationManager.AppSettings["Sender"];
                            string password = ConfigurationManager.AppSettings["Password"];
                            client.Credentials = new System.Net.NetworkCredential(sender, password);
                            string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
                            mail.From = new MailAddress(EmailFrom);

                            foreach (var address in Users.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                mail.To.Add(address);
                            }
                            mail.CC.Add(ccList);
                            mail.Subject = email != null ? email.Subject : "";
                            mail.Body = Body;
                            mail.IsBodyHtml = true;

                            string Parameters_three = "Inside SmtpClient";

                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(attachmentPath);
                            //attachment = new System.Net.Mail.Attachment(Server.MapPath("~/Penguins.jpg"));
                            mail.Attachments.Add(attachment);
                            client.Send(mail);
                        }

                        catch (Exception exception)
                        {
                            string Parameters_four = "Exception: Message : " + exception.Message + ", InnerException : " + exception.InnerException;
                        }
                        finally
                        {
                            string Parameters_six = "Finally: Message :";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string Parameters_five = "EXCEPTION: " + ex.Message + "STACK TRACE: " + ex.StackTrace;
            }
        }
        return Res;
    }
