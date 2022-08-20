using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services.Interfaces
{
    public interface IEmailService
    {
        string RegistrationSubject();
        string EditorSubject();
        string OperatorEditorSubject();
        string AgentSubject();
        string OperatorSubejct();
        string RegistrationContent();
        string EditorContent();
        string OperatorEditorContent(string agent, string time);
        string AgentContent(string user, string time);
        string OperatorContent(string agent, string time);
        void Email_to_send(string email, string url, string content, string subject);

    }
}
