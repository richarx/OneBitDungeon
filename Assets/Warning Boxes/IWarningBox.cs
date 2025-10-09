namespace Warning_Boxes
{
    public interface IWarningBox
    {
        public bool IsFull();
        public void Trigger();
        public void DestroyBox();
        public void Cancel();
    }
}
