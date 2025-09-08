using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces;

public interface IStockService
{
    /// <summary>
    /// Ghi nhận nhập/xuất/điều chỉnh. Quantity là số dương; service sẽ tự gán dấu theo MovementType.
    /// </summary>
    Task<(bool ok, string? error)> MoveAsync(StockMoveRequestDto req, long? createdBy, CancellationToken ct = default);
}