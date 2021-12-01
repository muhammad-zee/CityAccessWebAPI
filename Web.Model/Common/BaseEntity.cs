namespace Web.Model.Common
{
    public abstract class BaseEntity
    {
        protected abstract void Validate();

        protected string BaseId = "";
    }
}