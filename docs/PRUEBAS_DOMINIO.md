# Pruebas Unitarias - Dominio - Descrubrimientos
**Fecha:** 1ro de Abril de 2026 (01/04/2026)
**Autor:** Diego Iván Enríquez Morales

---

## Descripción

Este documento es el resultado de la primera ronda de pruebas unitarias para las clases del proyecto de Dominio (MemoryGame.Domain). Aqui, se resaltan los descubrimientos relevantes tras la ejecución de dichas pruebas.

---

## Cobertura de Pruebas

- Caminos Felices (Happy Paths).
- Errores de Dominio conocidas (caminos de excepción que arrojen DomainException).
- Casos de datos inválidos adicionales (caminos de error o excepción no considerados en la codificación temprana del proyecto de Dominio).

---

## Descubrimientos

- **Clase `Card`**
    - **`Create_DeckIdIsNotValidId_ThrowsDomainException()`**: No se valida ID de instancia `Deck`.
- **Clase `Deck`**
    - **`Create_MatchIdIsNotValidId_ThrowsDomainException()`**: No se valida ID de instancia `Match`.
- **Clase `Match`**
    - **`AddParticipant_MatchIdIsValid_ReturnNewMatchParticipation()`**: Actualmente, no existe forma de definir ID para instancias `Match`, ya sea en el constructor o en método `Create()`; la clase `BaseEntity` define el setter del atributo `Id` como `protected`.
- **Clase `Penalty`**
    - **`Create_UserIdIsNotValid_ThrowDomainException()`**: No se valida ID de instancia `User`.
    - **`Create_MatchIdIsNotValid_ThrowDomainException()`**: No se valida ID de instancia `Match`.
    - **`Create_DurationIsBeforePresentTime_ThrowDomainException()`**: No se valida que la fecha y hora de expiración de la instancia `Penalty` sea después de la fecha y hora actual o de creación de dicha instancia.
- **Clase `FriendRequest`**
    Pruebas:
    - **`Create_SenderIdIsNotValid_ThrowsDomainException()`**
    - **`Create_ReceiverIdIsNotValid_ThrowsDomainException()`**
    
    No se validan las IDs de las instancias `User`. 
- **Clase `Friendship`**
    Pruebas:
    - **`Create_FriendIdIsNotValid_ThrowsDomainException()`**
    - **`Create_UserIdIsNotValid_ThrowsDomainException()`**
    
    No se validan las IDs de las instancias `User`.
- **Clase `SocialNetwork`**
    - **`UpdateAccount_NewAccountNameIsTooLong_ThrowsDomainException()`**: No se valida la longitud en caracteres del nuevo nombre.
- **Clase `Email`**
    Pruebas:
    - **`Create_ValueDoesNotContainUsername_ThrowsDomainException()`**
    - **`Create_ValueDoesNotContainDomain_ThrowsDomainException()`**
    
    No se valida adecuadamente la estructura de la dirección.
    No se consideran los siguientes escenarios:
    - La ausencia del nombre de usuario (caracteres antes de "@").
    - La ausencia del dominio (caracteres después de "."; por ejemplo, ".com", ".mx").
- **Clase `PendingRegistration`**
    Pruebas:
    - **`Create_PinIsTooShort_ThrowsDomainException()`**
    - **`UpdatePin_PinDoesNotContainNumericalCharacters_ThrowDomainException()`**
    - **`Create_PinContainsAlphabeticalCharacters_ThrowDomainException()`**
    - **`CreateForUpgrade_PinIsTooShort_ThrowDomainException()`**
    - **`Create_PinDoesNotContainNumericCharacters_ThrowDomainException()`**
    - **`CreateForUpgrade_PinDoesNotContainNumericCharacters()`**
    - **`CreateForUpgrade_PinContainsAlphabeticalCharacters_ThrowDomainException()`**
    - **`UpdatePin_PinIsTooShort_ThrowDomainException()`**
    - **`UpdatePin_PinDoesContainsAlphabeticalCharacters_ThrowDomainException()`**
    
    No se valida adecuadamente las características del atributo `pin`; el pin es una cadena de 6 números, aunque se le agregó un limite de 10 caracteres.
    No se consideran los siguientes escenarios:
    - Pin demasiado corto (menos de 6 caracteres).
    - Pin demasiado largo (más de 6 caracteres).
    - Presencia de caracteres alfabéticos o ausencia total de caracteres numéricos.
- **Clase `User`**
    Pruebas:
    - **`UpdatePersonalInfo_LastNameIsWhiteSpace_ThrowDomainException()`**
    - **`UpdatePersonalInfo_NameIsNull_ThrowDomainException()`**
    - **`UpdatePersonalInfo_LastNameIsNull_ThrowDomainException()`**
    - **`UpdatePersonalInfo_NameIsWhiteSpace_ThrowDomainException()`**

    No se valida la entrada de campos nulos o compuestos únicamente por caracteres de espacio.

    Adicionalmente:
    - **`VerifyEmail_UserIsGuest_ThrowDomainException()`**: No se valida si el usuario es Invitado (`IsGuest = false`).
