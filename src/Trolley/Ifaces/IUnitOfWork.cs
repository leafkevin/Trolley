﻿namespace Trolley;

public interface IUnitOfWork
{
    void BeginTransaction();
    void Commit();
    void Rollback();
}
