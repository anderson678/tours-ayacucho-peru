// Tarea 4.1 â€” SD-05: Enum MetodoPago â€” TOURS AYACUCHO PERÃš
// Valores alineados 1:1 con RN-05-01 y el CHECK constraint de Pagos.MetodoPago en database/ToursAyacuchoPeru.sql
namespace ToursAyacuchoPeruAPI.Domain.Enums
{
    public enum MetodoPago
    {
        TransferenciaBancaria,
        DepositoCuenta,
        Yape,
        Plin
    }
}


