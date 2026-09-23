-- Dogfood fixture: exercises the Oracle PL/SQL analysis path for .sql files.
-- The VARCHAR column intentionally triggers plsql:S1192 ("Use VARCHAR2 instead of VARCHAR").
-- We can exclude this in the build.cake via sonar.exclusions (SonarArgs.AdditionalProperties)
-- to verify the extra-properties passthrough end-to-end in the Sonar scan.
CREATE TABLE Dogfood_Plsql_Scan (
    Id INT,
    Name VARCHAR(100)
);
