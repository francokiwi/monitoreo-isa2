# monitoreo-isa2

## Instrucciones para ejecutar el proyecto

1. Correr contenedores de docker
```sh
docker compose up -d
```

2. Ejecutar el proyecto de dotnet
```sh
dotnet run
```

## Explicacion de la solucion

La solucion consiste de una api web que expone la URI /rolldice, que devuelve un numero del 1 al 6 cuando recibe un GET.
Esta utiliza OpenTelemetryProtocol como exportador para enviar estadisticas de la app a Prometheus.

Las estadisticas que envia son:
- Contador de cantidad de tiradas
- Histograma de valores de las tiradas
