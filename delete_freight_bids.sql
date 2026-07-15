-- SQL query to delete all freight bids and their dependent records
-- This script uses a transaction to ensure atomicity

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Delete DriverBids that reference FreightBids
    DELETE FROM dbo.DriverBids
    WHERE FreightBidId IN (SELECT Id FROM dbo.FreightBids);
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' driver bids';

    -- 2. Delete FreightBidUseTags
    DELETE FROM dbo.FreightBidUseTags
    WHERE FreightBidId IN (SELECT Id FROM dbo.FreightBids);
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' freight bid use tags';

    -- 3. Delete FreightBidHelpOptions
    DELETE FROM dbo.FreightBidHelpOptions
    WHERE FreightBidId IN (SELECT Id FROM dbo.FreightBids);
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' freight bid help options';

    -- 4. Finally delete the FreightBids themselves
    DELETE FROM dbo.FreightBids;
    PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' freight bids';

    -- Commit the transaction if everything succeeds
    COMMIT TRANSACTION;
    PRINT 'All freight bids and dependent records successfully deleted';
END TRY
BEGIN CATCH
    -- Roll back the transaction if there's an error
    ROLLBACK TRANSACTION;
    
    -- Error information
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'An error occurred: ' + @ErrorMessage;
    
    -- Raise the error to the client
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH