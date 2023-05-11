FROM postgres:14

# Enabling pgaudit
RUN apt-get update && apt-get install -y --no-install-recommends \
    postgresql-$PG_MAJOR-pgaudit

# Cleanup
RUN apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*