namespace SharpDataAccess.Data
{
    public interface IDataScopeLogger
    {
        void ScopeCreating();
        
        void ScopeCreated();

        void ScopeReused();

        void ScopeDestroying();

        void ScopeDestroyed();
        
        void TransactionReused();
        
        void TransactionCreating();
        
        void TransactionCreated();
        
        void TransactionDestroying();
        
        void TransactionDestroyed();
    }
}