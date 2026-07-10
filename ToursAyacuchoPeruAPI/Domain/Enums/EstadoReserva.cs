// Tarea 4.1 â€” SD-04, SD-05, SD-06, SD-07: Enum EstadoReserva â€” TOURS AYACUCHO PERÃš
//
// IMPORTANTE: los nombres de los miembros son IDÃ‰NTICOS a los valores permitidos por el
// CHECK constraint de Reservas.Estado en database/ToursAyacuchoPeru.sql. Esto permite usar
// Enum.ToString()/Enum.Parse() directamente en el DbContext sin necesitar un mapeo
// adicional, evitando que el modelo de datos del cÃ³digo y el de la base de datos diverjan
// (el problema detectado en la versiÃ³n anterior del proyecto).
namespace ToursAyacuchoPeruAPI.Domain.Enums
{
    public enum EstadoReserva
    {
        PENDIENTE_PAGO,
        CONFIRMADA,
        REPROGRAMADA,
        COMPLETADA,
        CANCELADA
    }
}


