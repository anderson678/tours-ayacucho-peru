# Documento de Requisitos
## Plataforma Web de Gestión de Ventas y Reservas — TOURS AYACUCHO PERÚ

**Proyecto de Investigación Académica**
**Paradigma:** Specification-Driven Development (SSD) con OpenSpec
**Universidad:** Universidad Nacional de San Cristóbal de Huamanga (UNSCH)
**Escuela Profesional:** Ingeniería de Sistemas
**Docente:** Mg. Ing. Richard Zapata Casaverde
**Estudiante:** Anderson Roki Ochoa Medrano
**Año:** 2026

---

## Introducción

La agencia de viajes TOURS AYACUCHO PERÚ, ubicada en la ciudad de Ayacucho, requiere una plataforma web que digitalice y centralice sus procesos de ventas y reservas de paquetes turísticos. En la actualidad, la gestión se realiza de forma manual o mediante canales informales, lo que genera inconsistencias en la disponibilidad de asientos, errores en el registro de pagos y demoras en la atención al cliente.

Este documento constituye la **Single Source of Truth (SSOT)** del sistema bajo el paradigma **Specification-Driven Development (SSD)**. Cada requisito aquí expresado se convierte en un **Spec Delta**: un contrato técnico verificable que vincula directamente el comportamiento esperado del sistema con los criterios de aceptación medibles. El cumplimiento íntegro de estos Spec Deltas determina la **Adecuación Funcional** del producto, evaluada según la norma **ISO/IEC 25010**.

El alcance del MVP obligatorio abarca cuatro módulos críticos: **Autenticación**, **Reservas**, **Pagos** y **Reprogramación**. Los módulos de **Administración**, **Reseñas**, **Notificaciones avanzadas** y **Reportes** pertenecen al alcance extendido y se implementan después de estabilizar el MVP.

---

## Glosario

- **Sistema**: La plataforma web de gestión de ventas y reservas de TOURS AYACUCHO PERÚ.
- **Cliente**: Usuario registrado que puede consultar, reservar, pagar y reprogramar paquetes turísticos.
- **Administrador**: Usuario con privilegios de gestión sobre cuentas, paquetes, disponibilidad e informes.
- **Auth_Service**: Componente del Sistema responsable de la autenticación y autorización de usuarios.
- **Reservation_Service**: Componente del Sistema responsable de la gestión de reservas y control de disponibilidad.
- **Payment_Service**: Componente del Sistema responsable del registro de pagos y emisión de comprobantes digitales.
- **Rescheduling_Service**: Componente del Sistema responsable de la reprogramación de reservas dentro del plazo permitido.
- **Notification_Service**: Componente del Sistema responsable del envío de correos electrónicos y notificaciones al Cliente.
- **Token_JWT**: Credencial digital firmada emitida por el Auth_Service tras una autenticación exitosa, con validez temporal.
- **Paquete_Turístico**: Producto comercial de la agencia que agrupa destino, fechas, actividades, transporte y alojamiento.
- **Configuración_Portada**: Datos institucionales editables de la página de inicio: nombre comercial, subtítulo, logo, textos principales, estadísticas y foto principal de fondo.
- **Reserva**: Solicitud formal de un Cliente para adquirir un Paquete_Turístico en una fecha y con un número de asientos determinados.
- **Overbooking**: Condición en la que el número de reservas activas supera la capacidad disponible de un Paquete_Turístico.
- **Spec_Delta**: Contrato técnico verificable que describe el comportamiento esperado de una unidad de funcionalidad del Sistema.
- **Comprobante_Digital**: Documento electrónico generado por el Payment_Service que acredita el registro de un pago.
- **SUS**: Escala de Usabilidad del Sistema (*System Usability Scale*), instrumento de 10 ítems en escala Likert 1–5.
- **ISO/IEC_25010**: Norma internacional de calidad de producto de software; en este proyecto se evalúa la característica de Adecuación Funcional.
- **Ventana_de_Reprogramación**: Período de 12 horas antes de la fecha de inicio del Paquete_Turístico durante el cual el Cliente puede solicitar una reprogramación.
- **bcrypt**: Función de hash criptográfico utilizada para el almacenamiento seguro de contraseñas.
- **ACID**: Propiedades de transacciones de base de datos (Atomicidad, Consistencia, Aislamiento, Durabilidad) implementadas en Microsoft SQL Server para prevenir Overbooking.

---

## Requisitos

---

### Requisito 1 — Spec Delta: Registro de Cliente (RF01)

**Historia de Usuario:** Como visitante no registrado, quiero crear una cuenta en la plataforma, para poder acceder a los servicios de reserva y pago de paquetes turísticos de TOURS AYACUCHO PERÚ.

**Descripción General:** El Auth_Service debe permitir que un visitante proporcione sus datos personales y credenciales para crear una cuenta de Cliente verificada. Las contraseñas deben almacenarse mediante hash bcrypt. El correo electrónico debe ser único en el sistema.

**Actores:** Visitante (no autenticado), Auth_Service, Notification_Service.

**Precondiciones:** El visitante no posee una cuenta registrada con el correo electrónico proporcionado.

**Reglas de Negocio Estrictas:**
- RN-01-01: El correo electrónico DEBE tener formato válido (RFC 5322) y DEBE ser único en el Sistema.
- RN-01-02: La contraseña DEBE tener una longitud mínima de 8 caracteres, incluyendo al menos una letra mayúscula, un dígito y un carácter especial.
- RN-01-03: El Auth_Service DEBE almacenar la contraseña utilizando el algoritmo bcrypt con un factor de costo mínimo de 10.
- RN-01-04: El Auth_Service NO DEBE almacenar la contraseña en texto plano en ningún medio de persistencia.
- RN-01-05: El Notification_Service DEBE enviar un correo de bienvenida al correo registrado tras el registro exitoso.

**Endpoint API RESTful:** `POST /api/v1/auth/register`

#### Criterios de Aceptación

1. WHEN un visitante envía nombre completo, correo electrónico, contraseña y número de teléfono válidos, THE Auth_Service SHALL crear la cuenta de Cliente y devolver el código de estado HTTP 201 junto con el identificador único del Cliente.
2. WHEN el registro es exitoso, THE Notification_Service SHALL enviar un correo de bienvenida al correo electrónico registrado en un tiempo máximo de 60 segundos.
3. IF el correo electrónico proporcionado ya existe en el Sistema, THEN THE Auth_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 409 con un mensaje descriptivo del conflicto.
4. IF la contraseña no cumple con los requisitos de complejidad definidos, THEN THE Auth_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 422 con la lista de criterios incumplidos.
5. IF algún campo obligatorio (nombre, correo, contraseña, teléfono) está ausente en la solicitud, THEN THE Auth_Service SHALL devolver el código de estado HTTP 400 con la identificación del campo faltante.
6. THE Auth_Service SHALL almacenar la contraseña exclusivamente como hash bcrypt con factor de costo mínimo de 10, sin persistir el valor original en ningún medio.

---

### Requisito 2 — Spec Delta: Inicio de Sesión (RF02)

**Historia de Usuario:** Como Cliente registrado, quiero iniciar sesión con mis credenciales, para poder acceder a mis reservas, historial de pagos y demás funcionalidades personalizadas de la plataforma.

**Descripción General:** El Auth_Service debe verificar las credenciales del Cliente contra los registros almacenados y, en caso de éxito, emitir un Token_JWT firmado que autorice el acceso a los recursos protegidos del Sistema.

**Actores:** Cliente, Auth_Service.

**Precondiciones:** El Cliente posee una cuenta registrada y activa en el Sistema.

**Reglas de Negocio Estrictas:**
- RN-02-01: El Token_JWT DEBE tener un período de expiración de 8 horas a partir de su emisión.
- RN-02-02: El Token_JWT DEBE incluir en su payload el identificador del Cliente y su rol (Cliente o Administrador).
- RN-02-03: El Auth_Service DEBE bloquear temporalmente una cuenta durante 15 minutos tras 5 intentos fallidos consecutivos de inicio de sesión.
- RN-02-04: El Auth_Service NO DEBE revelar en el mensaje de error si la causa del fallo es el correo incorrecto o la contraseña incorrecta.

**Endpoint API RESTful:** `POST /api/v1/auth/login`

#### Criterios de Aceptación

1. WHEN un Cliente envía correo electrónico y contraseña correctos, THE Auth_Service SHALL devolver el código de estado HTTP 200 junto con un Token_JWT firmado y su tiempo de expiración.
2. THE Token_JWT SHALL contener en su payload el identificador único del Cliente, el rol asignado y la fecha de expiración.
3. IF las credenciales proporcionadas son incorrectas, THEN THE Auth_Service SHALL devolver el código de estado HTTP 401 con un mensaje genérico que no distinga entre correo incorrecto y contraseña incorrecta.
4. IF un Cliente realiza 5 intentos fallidos consecutivos de inicio de sesión, THEN THE Auth_Service SHALL bloquear la cuenta por un período de 15 minutos y devolver el código de estado HTTP 429 indicando el tiempo restante de bloqueo.
5. WHILE la cuenta de un Cliente está bloqueada, THE Auth_Service SHALL rechazar todo intento de inicio de sesión y devolver el código de estado HTTP 429 con el tiempo restante de bloqueo.
6. IF el correo electrónico proporcionado no corresponde a ninguna cuenta registrada, THEN THE Auth_Service SHALL devolver el código de estado HTTP 401 con el mismo mensaje genérico definido en el criterio 3.

---

### Requisito 3 — Spec Delta: Actualización de Perfil de Cliente (RF03)

**Historia de Usuario:** Como Cliente autenticado, quiero actualizar mis datos personales de contacto, para mantener mi información vigente en la plataforma.

**Descripción General:** El Sistema debe permitir que un Cliente o Administrador autenticado consulte y modifique su nombre, teléfono y foto de perfil. El correo electrónico no es modificable por este flujo para preservar la integridad de la identidad. Los cambios deben persistirse de forma inmediata y reflejarse en el encabezado y en la vista de Perfil.

**Actores:** Cliente, Auth_Service.

**Precondiciones:** El Cliente posee un Token_JWT válido y no expirado.

**Reglas de Negocio Estrictas:**
- RN-03-01: El Sistema NO DEBE permitir la modificación del correo electrónico mediante este endpoint.
- RN-03-02: El número de teléfono DEBE contener entre 9 y 15 dígitos numéricos.
- RN-03-03: La foto de perfil DEBE almacenarse como URL HTTP/HTTPS de longitud máxima 600 caracteres o quedar vacía si el usuario decide retirarla.

**Endpoints API RESTful:**
- `GET /api/v1/clients/{clientId}/profile`
- `PUT /api/v1/clients/{clientId}/profile`

#### Criterios de Aceptación

1. WHEN un Cliente o Administrador autenticado consulta su perfil, THE Auth_Service SHALL devolver nombre, correo, teléfono, rol y foto de perfil con código HTTP 200.
2. WHEN un Cliente o Administrador autenticado envía datos de perfil válidos (nombre, teléfono o foto), THE Auth_Service SHALL persistir los cambios y devolver el código de estado HTTP 200 con los datos actualizados.
3. IF la solicitud no incluye un Token_JWT válido, THEN THE Auth_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 401.
4. IF el usuario intenta modificar el correo electrónico en la solicitud, THEN THE Auth_Service SHALL ignorar el campo de correo electrónico y procesar únicamente los campos permitidos.
5. IF el número de teléfono proporcionado contiene caracteres no numéricos o una longitud fuera del rango de 9 a 15 dígitos, THEN THE Auth_Service SHALL devolver el código de estado HTTP 422 con la descripción del error de validación.
6. IF la URL de foto de perfil no usa HTTP/HTTPS o supera 600 caracteres, THEN THE Auth_Service SHALL devolver el código de estado HTTP 422.

---

### Requisito 4 — Spec Delta: Reserva de Paquetes Turísticos con Control de Overbooking (RF04, RF05)

**Historia de Usuario:** Como Cliente autenticado, quiero seleccionar un paquete turístico, indicar la cantidad de asientos y confirmar mi reserva, para asegurar mi participación en el tour deseado sin riesgo de overbooking.

**Descripción General:** El Reservation_Service debe gestionar el ciclo completo de una reserva: selección de paquete, validación de disponibilidad en tiempo real y confirmación atómica. La operación de descuento de asientos disponibles debe ejecutarse dentro de una transacción ACID para garantizar la consistencia de los datos ante solicitudes concurrentes y prevenir el Overbooking.

**Actores:** Cliente, Reservation_Service.

**Precondiciones:** El Cliente posee un Token_JWT válido. El Paquete_Turístico existe y se encuentra activo en el Sistema.

**Reglas de Negocio Estrictas:**
- RN-04-01: El Reservation_Service DEBE validar la disponibilidad de asientos en tiempo real mediante una transacción ACID antes de confirmar la Reserva.
- RN-04-02: El Reservation_Service NO DEBE confirmar una Reserva si el número de asientos solicitados supera los asientos disponibles del Paquete_Turístico.
- RN-04-03: La reducción de asientos disponibles DEBE ejecutarse de forma atómica junto con la creación del registro de Reserva dentro de la misma transacción de base de datos.
- RN-04-04: El estado inicial de toda Reserva confirmada DEBE ser "PENDIENTE_PAGO".
- RN-04-05: Un Cliente NO DEBE tener más de una Reserva activa con estado "PENDIENTE_PAGO" para el mismo Paquete_Turístico.

**Endpoint API RESTful:** `POST /api/v1/reservations`

#### Criterios de Aceptación

1. WHEN un Cliente autenticado envía el identificador del Paquete_Turístico, la fecha de inicio y la cantidad de asientos solicitados, THE Reservation_Service SHALL verificar la disponibilidad y, si hay asientos suficientes, crear la Reserva con estado "PENDIENTE_PAGO" y devolver el código de estado HTTP 201 con el identificador único de la Reserva.
2. THE Reservation_Service SHALL ejecutar la creación de la Reserva y el descuento de asientos disponibles dentro de una única transacción ACID, garantizando que ambas operaciones se apliquen o ninguna persista.
3. IF la cantidad de asientos solicitados supera los asientos disponibles en el Paquete_Turístico al momento de la validación, THEN THE Reservation_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 409 indicando la disponibilidad actual.
4. IF dos solicitudes de Reserva para el mismo Paquete_Turístico se reciben de forma concurrente y la disponibilidad es insuficiente para ambas, THEN THE Reservation_Service SHALL confirmar únicamente la primera transacción que adquiera el bloqueo y rechazar la segunda con código de estado HTTP 409.
5. WHEN un Cliente consulta sus reservas activas, THE Reservation_Service SHALL devolver la lista de Reservas con su estado actual, fechas y datos del Paquete_Turístico asociado.
6. IF la solicitud de Reserva no incluye un Token_JWT válido, THEN THE Reservation_Service SHALL devolver el código de estado HTTP 401.

---

### Requisito 5 — Spec Delta: Registro de Pago y Emisión de Comprobante Digital (RF06, RF07)

**Historia de Usuario:** Como Cliente autenticado, quiero registrar el pago de mi reserva y recibir automáticamente un comprobante digital en mi correo electrónico, para tener evidencia formal del pago realizado.

**Descripción General:** El Payment_Service debe registrar los datos del pago de una Reserva, actualizar el estado de la Reserva a "CONFIRMADA" y delegar al Notification_Service la emisión inmediata del Comprobante_Digital al correo del Cliente. El proceso debe ser transaccional para evitar inconsistencias entre el registro del pago y el cambio de estado de la Reserva.

**Actores:** Cliente, Payment_Service, Notification_Service.

**Precondiciones:** El Cliente posee un Token_JWT válido. La Reserva existe con estado "PENDIENTE_PAGO" y pertenece al Cliente autenticado.

**Reglas de Negocio Estrictas:**
- RN-05-01: El Payment_Service DEBE aceptar los métodos de pago: transferencia bancaria, depósito en cuenta, Yape y Plin.
- RN-05-02: El monto registrado DEBE ser igual al monto total del Paquete_Turístico multiplicado por el número de asientos reservados, con una tolerancia de S/ 0.01 por redondeo.
- RN-05-03: El Payment_Service DEBE cambiar el estado de la Reserva a "CONFIRMADA" dentro de la misma transacción ACID en la que se registra el pago.
- RN-05-04: El Notification_Service DEBE enviar el Comprobante_Digital al correo del Cliente en un tiempo máximo de 120 segundos tras la confirmación del pago.
- RN-05-05: El Comprobante_Digital DEBE contener: identificador de la Reserva, nombre del Paquete_Turístico, nombre del Cliente, monto pagado, método de pago, número de referencia y fecha y hora del registro.
- RN-05-06: Una Reserva con estado "CONFIRMADA" NO DEBE admitir un segundo registro de pago.

**Endpoints API RESTful:**
- `POST /api/v1/payments` — Registrar pago
- `GET /api/v1/payments/{paymentId}/receipt` — Obtener comprobante digital

#### Criterios de Aceptación

1. WHEN un Cliente autenticado envía el identificador de la Reserva, el monto, el método de pago y el número de referencia válidos, THE Payment_Service SHALL registrar el pago y actualizar el estado de la Reserva a "CONFIRMADA" dentro de una transacción ACID, devolviendo el código de estado HTTP 201 con el identificador del pago.
2. WHEN el pago es registrado exitosamente, THE Notification_Service SHALL enviar el Comprobante_Digital al correo electrónico del Cliente en un tiempo máximo de 120 segundos.
3. THE Comprobante_Digital SHALL contener el identificador de la Reserva, el nombre del Paquete_Turístico, el nombre del Cliente, el monto pagado, el método de pago, el número de referencia y la fecha y hora del registro del pago.
4. IF el monto enviado difiere en más de S/ 0.01 del monto calculado para la Reserva, THEN THE Payment_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 422 con el monto esperado.
5. IF la Reserva ya posee estado "CONFIRMADA" al momento de la solicitud, THEN THE Payment_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 409 indicando que la Reserva ya fue pagada.
6. IF el método de pago proporcionado no pertenece a los métodos aceptados, THEN THE Payment_Service SHALL devolver el código de estado HTTP 422 con la lista de métodos válidos.
7. IF la solicitud no incluye un Token_JWT válido, THEN THE Payment_Service SHALL devolver el código de estado HTTP 401.

---

### Requisito 6 — Spec Delta: Reprogramación de Reserva (CU N° 05)

**Historia de Usuario:** Como Cliente autenticado, quiero solicitar el cambio de fecha de mi reserva confirmada, para adaptar mi itinerario de viaje cuando sea necesario, siempre que lo haga con suficiente anticipación.

**Descripción General:** El Rescheduling_Service debe permitir que un Cliente solicite una nueva fecha para su Reserva confirmada, validando que la solicitud se realice dentro de la Ventana_de_Reprogramación (mínimo 12 horas antes de la fecha de inicio del tour) y que la nueva fecha tenga asientos disponibles. La operación debe ser transaccional: liberar asientos en la fecha original y reservarlos en la nueva fecha de forma atómica.

**Actores:** Cliente, Rescheduling_Service, Reservation_Service.

**Precondiciones:** El Cliente posee un Token_JWT válido. La Reserva existe con estado "CONFIRMADA" y pertenece al Cliente autenticado. La solicitud se realiza dentro de la Ventana_de_Reprogramación.

**Reglas de Negocio Estrictas:**
- RN-06-01: El Rescheduling_Service DEBE rechazar toda solicitud de reprogramación realizada con menos de 12 horas de anticipación a la fecha de inicio del Paquete_Turístico.
- RN-06-02: La nueva fecha solicitada DEBE ser posterior a la fecha actual del Sistema.
- RN-06-03: El Rescheduling_Service DEBE verificar la disponibilidad de asientos en la nueva fecha antes de ejecutar el cambio.
- RN-06-04: La liberación de asientos en la fecha original y la asignación en la nueva fecha DEBEN ejecutarse en una única transacción ACID.
- Nota técnica: en el MVP actual cada Paquete_Turístico representa una única salida fechada con disponibilidad propia. Si se requieren múltiples salidas por paquete, se deberá introducir una entidad SalidaPaquete/DisponibilidadPorFecha antes de ampliar RN-06-03 y RN-06-04.
- RN-06-05: El número máximo de reprogramaciones permitidas por Reserva DEBE ser 1.
- RN-06-06: El Notification_Service DEBE notificar al Cliente la confirmación de la reprogramación en un tiempo máximo de 60 segundos.

**Endpoint API RESTful:** `PATCH /api/v1/reservations/{reservationId}/reschedule`

#### Criterios de Aceptación

1. WHEN un Cliente autenticado solicita la reprogramación de una Reserva confirmada con una nueva fecha válida y con al menos 12 horas de anticipación a la fecha de inicio original, THE Rescheduling_Service SHALL verificar la disponibilidad en la nueva fecha y, si existe disponibilidad, actualizar la fecha de la Reserva dentro de una transacción ACID, devolviendo el código de estado HTTP 200 con los datos actualizados.
2. THE Rescheduling_Service SHALL ejecutar la liberación de asientos en la fecha original y la asignación de asientos en la nueva fecha dentro de una única transacción ACID, garantizando que ambas operaciones se apliquen o ninguna persista.
3. WHEN la reprogramación es confirmada, THE Notification_Service SHALL enviar al Cliente una notificación con los detalles de la nueva fecha en un tiempo máximo de 60 segundos.
4. IF la solicitud se realiza con menos de 12 horas de anticipación a la fecha de inicio del Paquete_Turístico, THEN THE Rescheduling_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 422 con el tiempo mínimo de anticipación requerido.
5. IF la nueva fecha solicitada no cuenta con asientos disponibles, THEN THE Rescheduling_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 409 con la disponibilidad de fechas alternativas.
6. IF la Reserva ya fue reprogramada previamente, THEN THE Rescheduling_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 422 indicando que el límite de reprogramaciones fue alcanzado.
7. IF la nueva fecha proporcionada es anterior o igual a la fecha actual del Sistema, THEN THE Rescheduling_Service SHALL devolver el código de estado HTTP 422 con la indicación de que la fecha debe ser futura.
8. IF la solicitud no incluye un Token_JWT válido, THEN THE Rescheduling_Service SHALL devolver el código de estado HTTP 401.

---

### Requisito 7 — Consulta del Estado de Reservas (RF08)

**Historia de Usuario:** Como Cliente autenticado, quiero consultar el estado actual de todas mis reservas, para hacer seguimiento de mis viajes planificados, pagados y completados.

**Descripción General:** El Reservation_Service debe proporcionar al Cliente una vista consolidada de todas sus reservas con su estado actualizado, datos del paquete turístico y, si aplica, el comprobante de pago asociado.

**Actores:** Cliente, Reservation_Service.

**Precondiciones:** El Cliente posee un Token_JWT válido.

**Reglas de Negocio Estrictas:**
- RN-07-01: El Sistema DEBE permitir filtrar las reservas por estado: PENDIENTE_PAGO, CONFIRMADA, REPROGRAMADA, COMPLETADA, CANCELADA.
- RN-07-02: El Sistema DEBE devolver únicamente las reservas pertenecientes al Cliente identificado en el Token_JWT.

**Endpoints API RESTful:**
- `GET /api/v1/reservations` — Listar reservas del Cliente
- `GET /api/v1/reservations/{reservationId}` — Consultar detalle de una Reserva

#### Criterios de Aceptación

1. WHEN un Cliente autenticado solicita la lista de sus reservas, THE Reservation_Service SHALL devolver el conjunto de Reservas asociadas al Cliente con estado HTTP 200, incluyendo para cada una: identificador, nombre del Paquete_Turístico, fechas, cantidad de asientos, estado actual y monto total.
2. WHERE el parámetro de filtro por estado es proporcionado, THE Reservation_Service SHALL devolver únicamente las Reservas que coincidan con el estado especificado.
3. THE Reservation_Service SHALL devolver exclusivamente las Reservas cuyo identificador de Cliente coincida con el identificador contenido en el Token_JWT de la solicitud.
4. IF la solicitud no incluye un Token_JWT válido, THEN THE Reservation_Service SHALL devolver el código de estado HTTP 401.

---

### Requisito 8 — Gestión de Cuentas de Cliente por Administrador (RF09)

**Historia de Usuario:** Como Administrador, quiero gestionar las cuentas de los clientes registrados, para mantener la integridad del padrón de usuarios y atender situaciones excepcionales como bloqueos o correcciones de datos.

**Descripción General:** El Auth_Service debe exponer operaciones de administración de cuentas restringidas al rol Administrador, incluyendo la consulta, activación, desactivación y eliminación lógica de cuentas de Cliente.

**Actores:** Administrador, Auth_Service.

**Precondiciones:** El Administrador posee un Token_JWT válido con rol "Administrador".

**Reglas de Negocio Estrictas:**
- RN-08-01: Las operaciones de gestión de cuentas DEBEN estar restringidas a usuarios con rol "Administrador" en el Token_JWT.
- RN-08-02: La eliminación de una cuenta DEBE ser lógica (marcado como inactiva), NO DEBE eliminar físicamente los registros de la base de datos.

**Endpoints API RESTful:**
- `GET /api/v1/admin/clients` — Listar cuentas de Clientes
- `PATCH /api/v1/admin/clients/{clientId}/status` — Activar o desactivar cuenta

#### Criterios de Aceptación

1. WHEN un Administrador autenticado solicita la lista de Clientes registrados, THE Auth_Service SHALL devolver el conjunto de cuentas con código de estado HTTP 200, incluyendo nombre, correo, estado y fecha de registro.
2. WHEN un Administrador autenticado envía una solicitud para cambiar el estado de una cuenta de Cliente, THE Auth_Service SHALL actualizar el estado de la cuenta y devolver el código de estado HTTP 200.
3. IF un Cliente con cuenta desactivada intenta iniciar sesión, THEN THE Auth_Service SHALL rechazar la solicitud y devolver el código de estado HTTP 403 con el mensaje "cuenta desactivada".
4. IF la solicitud de gestión no incluye un Token_JWT con rol "Administrador", THEN THE Auth_Service SHALL devolver el código de estado HTTP 403.

---

### Requisito 9 — Comentarios y Calificaciones sobre Tours (RF10)

**Historia de Usuario:** Como Cliente que ha completado un tour, quiero publicar un comentario y una calificación sobre el paquete turístico, para compartir mi experiencia con otros usuarios y contribuir a la reputación de la agencia.

**Descripción General:** El Sistema debe permitir que Clientes con reservas en estado "COMPLETADA" publiquen una reseña compuesta por calificación numérica y comentario textual sobre el Paquete_Turístico correspondiente.

**Actores:** Cliente, Sistema.

**Precondiciones:** El Cliente posee un Token_JWT válido. El Cliente tiene al menos una Reserva con estado "COMPLETADA" para el Paquete_Turístico a calificar.

**Reglas de Negocio Estrictas:**
- RN-09-01: La calificación DEBE ser un valor entero entre 1 y 5.
- RN-09-02: Un Cliente NO DEBE publicar más de una reseña por Paquete_Turístico.
- RN-09-03: Solo Clientes con Reservas en estado "COMPLETADA" para el Paquete_Turístico DEBEN poder publicar reseñas.

**Endpoint API RESTful:** `POST /api/v1/packages/{packageId}/reviews`

#### Criterios de Aceptación

1. WHEN un Cliente autenticado con una Reserva completada del Paquete_Turístico envía una calificación entre 1 y 5 y un comentario, THE Sistema SHALL registrar la reseña y devolver el código de estado HTTP 201.
2. IF el Cliente no tiene una Reserva con estado "COMPLETADA" para el Paquete_Turístico indicado, THEN THE Sistema SHALL rechazar la solicitud y devolver el código de estado HTTP 403.
3. IF el Cliente ya publicó una reseña para el mismo Paquete_Turístico, THEN THE Sistema SHALL rechazar la solicitud y devolver el código de estado HTTP 409.
4. IF la calificación enviada no es un entero entre 1 y 5, THEN THE Sistema SHALL devolver el código de estado HTTP 422.

---

### Requisito 10 — Gestión de Paquetes Turísticos e Itinerarios (RF11, RF12, RF13)

**Historia de Usuario:** Como Administrador, quiero gestionar los paquetes turísticos con sus itinerarios, precios, disponibilidad y ofertas, para que los Clientes puedan consultar información actualizada y realizar reservas sobre datos vigentes.

**Descripción General:** El Sistema debe proveer al Administrador operaciones CRUD sobre los Paquetes_Turísticos, incluyendo imagen, itinerario descriptivo, precio, disponibilidad y estado activo. Los visitantes y Clientes pueden consultar el catálogo público. En la portada se muestran únicamente 4 paquetes destacados/llamativos, mientras que el panel del Cliente incluye una sección "Paquetes" con todos los paquetes activos disponibles.

**Actores:** Administrador, Cliente, Sistema.

**Precondiciones (para operaciones de escritura):** El Administrador posee un Token_JWT con rol "Administrador".

**Reglas de Negocio Estrictas:**
- RN-10-01: El precio de un Paquete_Turístico DEBE ser un valor numérico positivo mayor a S/ 0.00.
- RN-10-02: La capacidad total de asientos DEBE ser un entero positivo.
- RN-10-03: La consulta del catálogo de paquetes activos DEBE estar disponible para visitantes no autenticados.
- RN-10-04: La portada DEBE mostrar como máximo 4 paquetes destacados para mantener una primera vista clara y comercial.
- RN-10-05: La sección "Paquetes" del panel del Cliente DEBE mostrar todos los paquetes activos obtenidos desde `GET /api/v1/packages`.

**Endpoints API RESTful:**
- `GET /api/v1/packages` — Listar paquetes activos (público)
- `GET /api/v1/packages/{packageId}` — Detalle del paquete (público)
- `POST /api/v1/admin/packages` — Crear paquete (Administrador)
- `PUT /api/v1/admin/packages/{packageId}` — Actualizar paquete (Administrador)
- `DELETE /api/v1/admin/packages/{packageId}` — Desactivar paquete (Administrador, eliminación lógica)

#### Criterios de Aceptación

1. WHEN cualquier visitante o Cliente autenticado solicita la lista de paquetes activos, THE Sistema SHALL devolver el catálogo con código de estado HTTP 200, incluyendo nombre, destino, descripción, imagen, precio, disponibilidad de asientos y fechas disponibles.
2. WHEN un Administrador autenticado crea un Paquete_Turístico con todos los campos requeridos válidos, THE Sistema SHALL persistir el paquete y devolver el código de estado HTTP 201 con el identificador generado.
3. WHEN un Administrador autenticado actualiza el precio o la disponibilidad de un Paquete_Turístico, THE Sistema SHALL reflejar los cambios en tiempo real en el catálogo público.
4. IF el precio proporcionado es menor o igual a S/ 0.00, THEN THE Sistema SHALL devolver el código de estado HTTP 422 con la descripción del error.
5. IF la operación de creación o actualización no incluye un Token_JWT con rol "Administrador", THEN THE Sistema SHALL devolver el código de estado HTTP 403.
6. WHEN la portada carga los paquetes destacados, THE Sistema SHALL mostrar únicamente los primeros 4 paquetes activos/promocionales.
7. WHEN un Cliente autenticado ingresa a la sección "Paquetes", THE Frontend SHALL listar todos los paquetes activos disponibles para consulta y reserva.

### Requisito 11 — Notificaciones al Cliente (RF15)

**Historia de Usuario:** Como Cliente, quiero recibir notificaciones por correo electrónico sobre eventos relevantes de mis reservas, para estar informado sin necesidad de ingresar constantemente a la plataforma.

**Descripción General:** El Notification_Service debe enviar correos electrónicos automáticos al Cliente ante los eventos de: registro exitoso, confirmación de pago, reprogramación confirmada y recordatorio 24 horas antes del inicio del tour.

**Actores:** Notification_Service, Cliente.

**Precondiciones:** El Cliente tiene una dirección de correo electrónico registrada y verificada.

**Reglas de Negocio Estrictas:**
- RN-11-01: El Notification_Service DEBE enviar notificación ante los eventos: registro de cuenta, pago confirmado, reprogramación confirmada y recordatorio de tour.
- RN-11-02: El recordatorio de tour DEBE enviarse 24 horas antes de la fecha de inicio del Paquete_Turístico.
- RN-11-03: El Notification_Service NO DEBE reenviar una notificación del mismo evento si ya fue entregada exitosamente.

**Endpoint API RESTful (interno):** Disparado por eventos de los servicios, sin endpoint público directo.

#### Criterios de Aceptación

1. WHEN el Auth_Service registra exitosamente una nueva cuenta, THE Notification_Service SHALL enviar un correo de bienvenida al correo registrado en un tiempo máximo de 60 segundos.
2. WHEN el Payment_Service confirma un pago, THE Notification_Service SHALL enviar el Comprobante_Digital al correo del Cliente en un tiempo máximo de 120 segundos.
3. WHEN el Rescheduling_Service confirma una reprogramación, THE Notification_Service SHALL enviar una notificación con la nueva fecha al correo del Cliente en un tiempo máximo de 60 segundos.
4. WHEN la fecha actual del Sistema alcanza las 24 horas previas a la fecha de inicio de una Reserva con estado "CONFIRMADA", THE Notification_Service SHALL enviar un correo de recordatorio al Cliente con los datos del tour.
5. IF un intento de envío de correo falla, THEN THE Notification_Service SHALL reintentar el envío hasta 3 veces con un intervalo de 5 minutos entre intentos.

---

### Requisito 12 — Reportes de Ventas y Reservas (RF17)

**Historia de Usuario:** Como Administrador, quiero generar reportes detallados de ventas y reservas en formatos PDF y Excel, para analizar el desempeño comercial de la agencia y tomar decisiones informadas.

**Descripción General:** El Sistema debe permitir al Administrador generar reportes filtrados por rango de fechas, estado de reservas o paquete turístico, exportables en formato PDF y Excel (XLSX).

**Actores:** Administrador, Sistema.

**Precondiciones:** El Administrador posee un Token_JWT con rol "Administrador".

**Reglas de Negocio Estrictas:**
- RN-12-01: Los reportes DEBEN estar disponibles exclusivamente para usuarios con rol "Administrador".
- RN-12-02: El rango de fechas del reporte DEBE ser proporcionado por el Administrador; la fecha de inicio DEBE ser anterior o igual a la fecha de fin.
- RN-12-03: El Sistema DEBE generar el archivo del reporte en un tiempo máximo de 30 segundos para rangos de hasta 12 meses.

**Endpoints API RESTful:**
- `GET /api/v1/admin/reports/reservations?from={date}&to={date}&format={pdf|xlsx}` — Reporte de reservas
- `GET /api/v1/admin/reports/sales?from={date}&to={date}&format={pdf|xlsx}` — Reporte de ventas

#### Criterios de Aceptación

1. WHEN un Administrador autenticado solicita un reporte con un rango de fechas válido y un formato de exportación, THE Sistema SHALL generar y devolver el archivo en el formato solicitado (PDF o XLSX) con código de estado HTTP 200 en un tiempo máximo de 30 segundos.
2. THE reporte de reservas SHALL contener para cada reserva: identificador, nombre del Cliente, Paquete_Turístico, fecha, asientos, estado y monto.
3. THE reporte de ventas SHALL contener el resumen de ingresos totales, número de reservas confirmadas y desglose por Paquete_Turístico para el período seleccionado.
4. IF la fecha de inicio del rango es posterior a la fecha de fin, THEN THE Sistema SHALL devolver el código de estado HTTP 422 con la descripción del error de validación.
5. IF la solicitud no incluye un Token_JWT con rol "Administrador", THEN THE Sistema SHALL devolver el código de estado HTTP 403.

---

### Requisito 13 — Configuración Editable de Portada e Identidad Visual (RF18)

**Historia de Usuario:** Como Administrador, quiero editar la portada, el logo y la identidad visual de la empresa, para mantener una presentación profesional y consistente para todos los usuarios.

**Descripción General:** El Sistema debe persistir la configuración principal de la portada en base de datos para que todos los usuarios vean la misma información desde cualquier equipo. La página de inicio debe usar una sola imagen principal de fondo configurable y el panel administrativo debe permitir actualizar textos, logo, estadísticas y foto principal.

**Actores:** Administrador, Visitante, Cliente, Sistema.

**Precondiciones (para operaciones de escritura):** El Administrador posee un Token_JWT con rol "Administrador".

**Reglas de Negocio Estrictas:**
- RN-13-01: La configuración de portada DEBE persistirse en la tabla `ConfiguracionPortada`.
- RN-13-02: La lectura pública de la configuración DEBE estar disponible sin autenticación.
- RN-13-03: La actualización de la configuración DEBE estar restringida al rol "Administrador".
- RN-13-04: La portada DEBE usar una sola imagen principal de fondo, tomada del primer elemento de `HeroImagesJson`.
- RN-13-05: Las URLs de logo e imagen principal DEBEN aceptar únicamente HTTP/HTTPS y una longitud máxima de 600 caracteres.

**Endpoints API RESTful:**
- `GET /api/v1/site-settings` — Obtener configuración pública de portada
- `GET /api/v1/admin/site-settings` — Obtener configuración para administración
- `PUT /api/v1/admin/site-settings` — Actualizar configuración de portada

#### Criterios de Aceptación

1. WHEN un Visitante o Cliente carga la portada, THE Sistema SHALL obtener la configuración desde `GET /api/v1/site-settings` y mostrar el logo, nombre de empresa, textos, estadísticas e imagen principal.
2. WHEN un Administrador actualiza la configuración desde el panel "Portada", THE Sistema SHALL persistir los cambios en SQL Server y devolver HTTP 200 con la configuración actualizada.
3. IF un usuario sin rol "Administrador" intenta actualizar la portada, THEN THE Sistema SHALL devolver HTTP 403.
4. IF la API pública de configuración no responde, THEN THE Frontend SHALL usar la configuración cacheada/local como respaldo visual sin bloquear la portada.
5. WHEN la configuración se actualiza correctamente, THE Header y Home SHALL refrescar la identidad visual sin requerir reinstalar ni modificar archivos del frontend.

---

## Requisitos No Funcionales

### RNF01 — Disponibilidad del Sistema

**Historia de Usuario:** Como Cliente, quiero que la plataforma esté disponible en todo momento, para poder realizar reservas y consultas sin restricción de horario.

#### Criterios de Aceptación

1. THE Sistema SHALL mantener una disponibilidad operativa mínima del 99.5% medida mensualmente, equivalente a un tiempo máximo de inactividad no planificada de 3 horas y 39 minutos por mes.
2. WHEN el Sistema detecta un fallo en un componente no crítico, THE Sistema SHALL continuar operando los módulos restantes sin interrupción total del servicio.

---

### RNF02 — Seguridad y Protección de Datos

**Historia de Usuario:** Como Cliente, quiero que mis datos personales y financieros estén protegidos, para confiar en que la plataforma no expondrá mi información sensible.

#### Criterios de Aceptación

1. THE Auth_Service SHALL transmitir todos los datos entre el cliente y el servidor exclusivamente mediante el protocolo HTTPS con TLS 1.2 o superior.
2. THE Auth_Service SHALL almacenar las contraseñas de los Clientes exclusivamente como hash bcrypt con factor de costo mínimo de 10.
3. THE Sistema SHALL incluir en cada Token_JWT una firma digital que permita verificar su integridad antes de procesar cualquier solicitud autenticada.

---

### RNF03 — Respaldo de Datos

**Historia de Usuario:** Como Administrador, quiero que los datos del sistema se respalden periódicamente, para poder recuperar la información en caso de un incidente.

#### Criterios de Aceptación

1. THE Sistema SHALL ejecutar copias de seguridad automáticas de la base de datos con una frecuencia mínima de una vez cada 24 horas.
2. WHEN un proceso de respaldo falla, THE Sistema SHALL generar una alerta al Administrador con el detalle del error en un tiempo máximo de 15 minutos.

---

### RNF04 — Capacidad y Rendimiento bajo Carga

**Historia de Usuario:** Como Administrador, quiero que la plataforma soporte múltiples usuarios simultáneos sin degradación del servicio, para garantizar una experiencia consistente durante períodos de alta demanda.

#### Criterios de Aceptación

1. THE Sistema SHALL procesar las solicitudes de consulta de paquetes turísticos con un tiempo de respuesta menor a 2 segundos cuando el número de usuarios concurrentes sea de hasta 100.
2. THE Sistema SHALL procesar las solicitudes de creación de reserva con un tiempo de respuesta menor a 3 segundos bajo una carga de 100 usuarios concurrentes.

---

### RNF05 — Usabilidad

**Historia de Usuario:** Como Cliente, quiero que la plataforma sea fácil de usar, para completar el proceso de reserva y pago sin necesidad de asistencia externa.

#### Criterios de Aceptación

1. THE Sistema SHALL obtener una puntuación mínima de 80 puntos en la Escala SUS aplicada a una muestra de 20 sujetos (16 Clientes y 4 Administradores), clasificando la usabilidad como "Excelente".
2. THE Sistema SHALL permitir que un Cliente complete el flujo de reserva y pago (desde selección de paquete hasta registro de pago) en un máximo de 5 pasos dentro de la interfaz.

---

### RNF06 — Compatibilidad Multiplataforma

**Historia de Usuario:** Como Cliente, quiero acceder a la plataforma desde cualquier dispositivo y sistema operativo, para no estar limitado a un equipo específico.

#### Criterios de Aceptación

1. THE Sistema SHALL renderizar y operar correctamente en los navegadores Google Chrome (versión 110 o superior), Mozilla Firefox (versión 110 o superior) y Microsoft Edge (versión 110 o superior).
2. THE Sistema SHALL adaptar su interfaz de forma responsiva a resoluciones de pantalla desde 360 píxeles de ancho (dispositivos móviles) hasta 1920 píxeles de ancho (escritorio).


---

## Resumen de Spec Deltas por Módulo MVP

| Spec Delta | Módulo | RFs cubiertos | Endpoints principales |
|---|---|---|---|
| SD-01: Registro de Cliente | Autenticación | RF01 | `POST /api/v1/auth/register` |
| SD-02: Inicio de Sesión | Autenticación | RF02 | `POST /api/v1/auth/login` |
| SD-03: Actualización de Perfil | Autenticación | RF03 | `PUT /api/v1/clients/{id}/profile` |
| SD-04: Reserva con Control de Overbooking | Reservas | RF04, RF05 | `POST /api/v1/reservations` |
| SD-05: Registro de Pago y Comprobante Digital | Pagos | RF06, RF07 | `POST /api/v1/payments` |
| SD-06: Reprogramación de Reserva | Reprogramación | CU N°05 | `PATCH /api/v1/reservations/{id}/reschedule` |
| SD-07: Consulta de Estado de Reservas | Reservas | RF08 | `GET /api/v1/reservations` |
| SD-08: Gestión de Cuentas (Administrador) | Autenticación | RF09 | `GET/PATCH /api/v1/admin/clients` |
| SD-09: Comentarios y Calificaciones | Catálogo | RF10 | `POST /api/v1/packages/{id}/reviews` |
| SD-10: Gestión de Paquetes e Itinerarios | Catálogo | RF11, RF12, RF13 | Público: `GET /api/v1/packages`; administración: `GET/POST/PUT/DELETE /api/v1/admin/packages` |
| SD-11: Notificaciones | Transversal | RF15 | (Evento interno) |
| SD-12: Reportes de Ventas y Reservas | Administración | RF17 | `GET /api/v1/admin/reports/*` |
| SD-13: Configuración de Portada | Administración / Portada | RF18 | `GET/PUT /api/v1/*/site-settings` |

---

## Trazabilidad con ISO/IEC 25010 — Adecuación Funcional

| Subcaracterística ISO 25010 | Spec Deltas Relacionados |
|---|---|
| Completitud Funcional | MVP obligatorio: SD-01 a SD-07. Alcance extendido: SD-08 a SD-12. |
| Corrección Funcional | SD-04 (ACID/Overbooking), SD-05 (integridad de pago), SD-06 (transacción de reprogramación) |
| Pertinencia Funcional | SD-01, SD-02 (autenticación mínima necesaria), SD-12 (reportes operacionales) |

*La verificación de Adecuación Funcional se realizará mediante el Checklist de verificación (Cumple/No Cumple) aplicado a cada criterio de aceptación de los Spec Deltas, con meta de cumplimiento del 100%.*

---

*Documento generado bajo el paradigma Specification-Driven Development (SSD) con OpenSpec.*
*Este documento constituye la Single Source of Truth (SSOT) del proyecto TOURS AYACUCHO PERÚ.*
## Estado de implementación verificado

- Los 13 Spec Deltas cuentan con implementación funcional en la API y/o el frontend, según corresponda.
- La API usa ASP.NET Core .NET 10, Entity Framework Core y SQL Server; el frontend usa React con Vite y archivos JavaScript/JSX.
- El esquema oficial es `database/ToursAyacuchoPeru.sql`, con nueve tablas. El script debe ejecutarse sobre una base vacía creada previamente.
- La verificación automatizada actual contiene 46 casos backend entre pruebas unitarias y de integración; todos pasaron en la última ejecución del 14 de julio de 2026.
- Permanecen como validaciones pendientes las propiedades PBT definidas en el diseño, las pruebas con SQL Server real, las pruebas de carga y la validación manual completa de responsividad y despliegue.

*Versión: 1.1 — Actualizado: 14 de julio de 2026*
