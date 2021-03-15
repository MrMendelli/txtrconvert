namespace txtrconvert.Impl
{
    public interface ICopyable<T, L>
    {
        #region Copy

        public void CopyTo(in T iCln);

        public void CopyTo(in T iCln, L cLvl);

        public void CopyFrom(in T iCln);

        public void CopyFrom(in T iCln, L cLvl);

        public void Copy(in T iCln, L cLvl, bool toOrFrom);

        #endregion

        #region Clone

        public T Clone();

        public void Clone(out T oCln);

        public T ClonePartial(L cLvl);

        public void ClonePartial(out T oCln, L cLvl);

        #endregion
    }
}
