# Customize Unity FPS Template Project

# Modified File List
## EnemyMobile.cs -> EnemyMobileRandom.cs
- Move random in "Patrol Mode"
- Move random abound target enemy in "Attack Mode"
## EnemyController.cs -> EnemyControllerRandom.cs
- Addapt for EnemyMobileRandom.cs
## EnemyManager.cs -> EnemyManagerRandom.cs
- Addapt for EnemyManager.cs
## EnemyTurret -> EnemyTurretRandom.cs
- Addapt for EnemyControllerRandom.cs

# Added File List
## EnemySpawner.cs
- Spawn specified prefab each specified span
## MapManager.cs
- Generate WP that refered by EnemyMovileRandom
- Generate a random position inside NavMesh