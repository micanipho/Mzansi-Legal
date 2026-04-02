using Abp.Dependency;
using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagIndexStore : ISingletonDependency
{
    private readonly object _syncRoot = new();
    private IReadOnlyList<IndexedChunk> _loadedChunks = Array.Empty<IndexedChunk>();
    private IReadOnlyList<DocumentProfile> _documentProfiles = Array.Empty<DocumentProfile>();

    public IReadOnlyList<IndexedChunk> LoadedChunks
    {
        get
        {
            lock (_syncRoot)
            {
                return _loadedChunks;
            }
        }
    }

    public IReadOnlyList<DocumentProfile> DocumentProfiles
    {
        get
        {
            lock (_syncRoot)
            {
                return _documentProfiles;
            }
        }
    }

    public bool IsReady
    {
        get
        {
            lock (_syncRoot)
            {
                return _loadedChunks.Count > 0 && _documentProfiles.Count > 0;
            }
        }
    }

    public void Replace(
        IReadOnlyList<IndexedChunk> loadedChunks,
        IReadOnlyList<DocumentProfile> documentProfiles)
    {
        Guard.Against.Null(loadedChunks, nameof(loadedChunks));
        Guard.Against.Null(documentProfiles, nameof(documentProfiles));

        lock (_syncRoot)
        {
            _loadedChunks = loadedChunks.ToList();
            _documentProfiles = documentProfiles.ToList();
        }
    }
}
