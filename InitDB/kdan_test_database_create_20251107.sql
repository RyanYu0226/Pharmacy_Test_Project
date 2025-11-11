-- 建立User
DO
$$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles WHERE rolname = 'kdan_pharmacy_db'
   ) THEN
      CREATE ROLE kdan_pharmacy_db WITH LOGIN PASSWORD 'pharmacydb@1234';
   END IF;
END
$$;

-- Database: KDAN_TEST
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'KDAN_TEST'
  AND pid <> pg_backend_pid();  -- 排除自己

DROP DATABASE IF EXISTS "KDAN_TEST";

CREATE DATABASE "KDAN_TEST"
    WITH
    OWNER = kdan_pharmacy_db
    ENCODING = 'UTF8'
    LC_COLLATE = 'Chinese (Traditional)_Taiwan.950'
    LC_CTYPE = 'Chinese (Traditional)_Taiwan.950'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
	

