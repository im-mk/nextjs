FROM liquibase/liquibase:5.0.1

COPY . /migration

RUN liquibase lpm add postgresql --global