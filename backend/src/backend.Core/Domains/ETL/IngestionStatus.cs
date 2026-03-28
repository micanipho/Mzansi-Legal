namespace backend.Domains.ETL;

/// <summary>
/// Represents the current pipeline stage of an IngestionJob.
/// The pipeline flows: Queued → Extracting → Transforming → Loading → Completed.
/// Any stage may transition to Failed on an unrecoverable error.
/// </summary>
public enum IngestionStatus
{
    Queued       = 0,
    Extracting   = 1,
    Transforming = 2,
    Loading      = 3,
    Completed    = 4,
    Failed       = 5
}
