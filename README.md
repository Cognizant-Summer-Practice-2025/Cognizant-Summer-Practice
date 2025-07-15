# Cognizant Summer Practice - Podman Quickstart


## 1. Pull the Required Postgres Image

Before starting the services, manually pull the Postgres 15 image:

```sh
podman pull docker.io/library/postgres:15
```

This ensures the image is available locally and avoids authentication issues during compose up.

---

## 2. Start All Services

Bring up all services in the background (detached mode):

For Linux / macOS

```sh
podman compose up -d
```
For Windows:

```sh
python -m podman_compose up -d
```


- This will build and start all containers defined in `docker-compose.yml`.

---

## 3. Stop and Remove All Services

To stop and remove all running containers, networks, and (optionally) volumes:

```sh
podman compose down
```

- Stops and removes containers and networks created by `up`.

To also remove named volumes:

```sh
podman compose down -v
```

- The `-v` flag removes named volumes declared in the `volumes` section of your compose file.

---

## 4. Clean Up All Podman Resources (Dangerous!)

To remove **all** unused containers, images, and volumes from your system:

```sh
podman system prune -a --volumes
```

- This will delete **all** stopped containers, unused images, and all volumes. Use with caution!

---
