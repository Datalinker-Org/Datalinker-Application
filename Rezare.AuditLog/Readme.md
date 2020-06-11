# Rezare Audit Log Library

This audit logging library uses X.509 certificates to sign audit messages
before writing them to the database. The signature can then be used in the
future to verify that none of the audit data has changed.

## Configuration

To use this in your application, first add the following to your application's
.config file (in the configSections node):

```
<section name="auditlog" type="Rezare.AuditLog.Config.ConfigurationSectionHandler, Rezare.AuditLog" />
```

And then add the section:

```
<auditlog>
	<enabled>True</enabled>
	<connectionString>CONNECTION_STRING</connectionString>
	<x509StoreName>STORE_NAME</x509StoreName>
	<x509StoreLocation>STORE_LOCATION</x509StoreLocation>
	<x509CertificateSubject>CERTIFICATE_SUBJECT</x509CertificateSubject>
	<x509CertificateFilePath>CERTIFICATE_FILE_PATH</x509CertificateFilePath>
	<hashAlgorithm>HASH_ALGORITHM</hashAlgorithm>
</auditlog>
```

The `enabled` tag determines if the Audit Logging framework will be active
or not. Generally this shold always be `True`, except for test project
configurations where Audit Logging should be transparently turned off.

The `CONNECTION_STRING` should point to the database that holds, or will hold,
the audit log database table and the stream sequences. The library will take
care of updating the database objects as required.

The next three values work together: `STORE_NAME`, `STORE_LOCATION` and 
`CERTIFICATE_SUBJECT`.

`STORE_NAME` must be one of:

* AddressBook
* AuthRoot
* CertificateAuthrority
* Disallowed
* My
* Root
* TrustedPeople
* TrustedPublisher

Other values will cause an exception. These correspond to the StoreName enum
(see: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx).

`STORE_LOCATION` must be one of:

* CurrentUser
* LocalMachine

Other values will cause an exception. These correspond to the StoreLocation enum
(see: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx).

`CERTIFICATE_SUBJECT` is the subject field, or substring of, for the certificate
to use for signing and verifying signatures.

The last value, `CERTIFICATE_FILE_PATH` is used if the certificate is stored in
a file instead of in the X.509 certificate store.

The X.509 certificate store take precedence over the file path, so if you need
to load the certificate from a file then leave the three config items 
`STORE_NAME`, `STORE_LOCATION` and  `CERTIFICATE_SUBJECT` empty.

`HASH_ALGORITHM` will indicate which of the hashing algorithms to use. Current
valid options are `SHA1`, `SHA2`, or `SHA256`. This is determined by what
signing algorithm your chosen X.509 certificate supports.