# Admin_Pdv_Lauren

Programme de communication avec le WS

**<u>Principe de fonctionnement : </u>**
* Script exécuté par l'admin PDV toutes les 10 minutes (Uniquement sur hyper-V)
* Chaque exécution met à jour notre base pour dire que le PDV est joignable
* Il va communiquer avec le WebService afin de savoir si des traitements sont à exécuter
* Les script à exécuter / Les fichiers à déposer ... sont stockée en centrale sur un serveur d'upload nodeJS
* Il va gérer l'ensemble des traitements (exécution, remonté des donnée, gestion des erreurs)
* L'ensemble des traitements à exécuter sont paramétrés dans notre base. (voir detail des traitement plus bas)
* Il va communiquer en permanance avec le WS afin d'avoir un suivi exact de l'avancement des traitements et des erreurs pour chaque magasin.

**<u>Traitement possible :</u>** 
* Dépose de fichier
* Dépose + exécution d'un fichier
* Exécution de script
* remontée de données dans des fichier plat par pdv sur nos serveur
* remontée de données concaténées tout pdv dans un fichier plat
* remontée de données dans notre base
* Plannification de traitement 

**<u>Principaux avantages : </u>**
* Script/fichier stockés en centrale sur un serveur d'upload/Download node JS (non bloquant)
* Suivi des traitement très précis (avec un grand nombre de code erreur)
* Un grand nombre de contrôle est effectué pour être sur que tout s'est bien déroulé
* Suivi en temps reel de l'avancement
* Rapidité de traitement
* Fiabilité
* Toute les interaction avec la base passent par des procédures stockée

**<u>Risques/désavantage : </u>**
* Charge réseau trop importante (A voir)

-----

### Détail des traitements : 

**<u>Traitement globale (toutes les 10 minutes) : </u>**
1. Appel du WS pour signaler ça présence
2. Le WS met a jour la table `RECEPTEUR_STATUS`, si le pdv (codehex) n'est pas présent il sera créé sinon on met a jour la ligne.
    * `writeBDD.WriteToBDD("EXEC Admin_PDV_WS.dbo.PS_STATUT_RECEPTEUR '" + codehex + "'", codehex, "", "");`
3. Purge + Suppression des répertoires : 
    * `public static string repDest = @"C:\TEMP\PRG_P3\";`
    * `public static string repResult = @"C:\TEMP\LOGP3\";` 
4. Demande au WS Si quelque chose est à faire `string fileDownload = ws.QueFaire(codehex);`
5. Le WS va exécuter la PS : `result = myExecQuery.EXEC_QUERY("Admin_PDV_WS.dbo.PS_ACTION_PLANNIF '" + codehex + "'");`
6. La PS retourne une ligne par programme à télécharger avec les infos suivantes: 
    * L'id du scénario
    * L'info si il faut remonter des données
    * Le nom du programme
    * Le lien de téléchargement (http://ip:3000/telecharger/nom_du_scenario/...)
    * Si oui ou non il s'agit du programme qui sera exécuter
    * Le nom du fichier à remonter (si pas de fichier a remonter : N/A)
    * Le type de remonté (BDD/FILE) (si pas de fichier a remonter : N/A)
    * Le nom que le fichier portera une fois remonté (si pas de fichier a remonter OU intégration en BDD : N/A)
    * L'endroit ou déposer le fichier sur notre serveur (si pas de fichier a remonter OU intégration en BDD : N/A)
    * Le format de remonté (PDV/CONCAT) (si pas de fichier a remonter OU intégration en BDD : N/A)
7. La PS met également à jour le statut du recepteur pour ce scénario (APPEL)
8. Création côté PDV des répertoires de travail 
    * `public static string repDest = @"C:\TEMP\PRG_P3\";`
    * `public static string repResult = @"C:\TEMP\LOGP3\";`     
9. Téléchargement des fichiers avec mise à jour du statut du récepteur
10. Stockage des informations remontées par la PS
11. Exécution du programme indiqué par la PS avec mise a jour du statut du récepteur
12. Traitement de remonté des données (si la PS a renvoyé l'info de remontée = 1)
    * BDD
    * FILE
13. Purge des répertoires de travail
14. Remonté du statut final et du code erreur via le WS.

**<u>Différentes étapes mise à jour : </u>**
    1. ATTENTE APPEL
    2. APPEL
    3. DOWNLOAD
    4. EXECUTION
    5. REMBDD
    6. REMFILE
    7. TERMINE
    8. TERMINE VIDE
    9. ERREUR

Chaque étape est mise à jour dans la table de suivi (`SCENARIO_RECEPTEUR`) avant que le traitement commence.</br>
Si le code erreur lors de l'exécution d'une étape est différent de 0 on ne passe pas à l'étape suivante.

**<u>Différent code erreur : </u>**
    * 1 = suppression en erreur des répertoire
    * 2 = erreur de téléchargement d'un fichier
    * 3 = erreur d'exécution du program
    * 4 = WS en erreur
    * 5 = Pas de traitement à faire
    * 6 = maj ws etape download
    * 7 = maj ws etape execution
    * 8 = maj ws etape terminée
    * 9 = maj ws code erreur
    * 10 = erreur remonté fichier vie ws
    * 11 = fichier de résultat attendu est introuvable
    * 12 = erreur sur la remontée du fichier de résultat
    * 13 = erreur d'importation sql
    * 14 = erreur de déplacement des fichier sur le serveur prod
    * 15 = création impossible du directory sur le serveur prod
    * 16 = maj WS etape rem BDD
    * 17 = maj WS etape rem File
    * 18 = Ecriture du fichier
    * 19 = La remontée est vide

Si nous avons le code erreur 19 lors de l'exécution d'un script celui-ci sera transformé en code erreur 0 et l'étape sera la 8 au lieu de 7 pour indiquer que tout s'est bien passé mais que aucun résultat n'a été généré.



-----


### Paramètrage d'un scénario : 

**<u>Paramètrage commun : </u>**
1. paramètrage de la table SCENARIO_ENTETE
    * le champ remonté indique si Admin_Pdv_Lauren doit ou non remonter des données
2. paralètrage de la table SCENARIO_LIGNE
    * Contient l'ensemble des fichiers à télécharger ainsi que l'infomartion du programme qui sera exécuté


**<u>Paramètrage pour une remontée en BDD : </u>**
1. Paramètrage de la table SCENARIO_REM
    * Contient le type de remonté (BDD/FILE)
    * Contient le nom du fichier à récupérer
2. Paramètrage de la table REMONTEE_BDD
    * Contient le nom de la base / schema / table

**<u>Paramètrae pour une remontée en fichier plat : </u>**
1. Paramètrage de la table SCENARIO_REM
    * Contient le type de remonté (BDD/FILE)
    * Contient le nom du fichier à récupérer
2. Paramètrage de la table REMONTEE_FILE
    * Contient le nom du fichier qui sera créé
    * Contient le chemin ou sera stocké le fichier
    * Contient le format d'écriture (PDV / CONCAT)


**<u>Paramètrage des PDV's cible : </u>**
Doit être fait après avoir tout paramétré.
1. Paramètrage de la table SCENARIO_RECEPTEUR
    * Indiquer le pdv
    * Indiquer le codehex