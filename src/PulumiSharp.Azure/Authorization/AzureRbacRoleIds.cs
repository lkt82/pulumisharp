namespace PulumiSharp.Azure.Authorization;

public static class AzureRbacRoleIds
{
    #region General

    public const string Owner = "8e3af657-a8ff-443c-a75c-2fe8c4bcb635";

    public const string Contributor = "b24988ac-6180-42a0-ab88-20f7382dd24c";

    public const string Reader = "acdd72a7-3385-48ef-bd42-f606fba81ae7";

    public const string UserAccessAdministrator = "18d7d88d-d35e-4fb5-a5c3-7773c20a72d9";

    #endregion

    #region Networking

    public const string NetworkContributor = "4d97b98b-1d4f-4787-a291-c67834d212e7";

    #endregion

    #region Compute

    public const string VirtualMachineContributor = "9980e02c-c2be-4d73-94e8-173b1dc7cf3c";

    #endregion

    #region Monitor

    public const string ApplicationInsightsComponentContributor = "ae349356-3a1b-4a5e-921d-050484c6347e";
    public const string ApplicationInsightsSnapshotDebugger = "08954f03-6346-4c2e-81c0-ec3a5cfae23b";
    public const string MonitoringContributor = "749f88d5-cbae-40b8-bcfc-e573ddc772fa";
    public const string MonitoringMetricsPublisher = "3913510d-42f4-4e42-8a64-420c390055eb";
    public const string MonitoringReader = "43d0d8ad-25c7-4714-9337-8ba259a9fe05";
    public const string WorkbookContributor = "e8ddcd69-c73f-4f9f-9844-4100522f16ad";
    public const string WorkbookReader = "b279062a-9be3-42a0-92ae-8b3cf002ec4d";

    #endregion

    #region Identity

    public const string ManagedIdentityOperator = "f1a07417-d97a-45cb-824c-7a7467783830";

    #endregion

    #region Security

    public const string KeyVaultAdministrator = "00482a5a-887f-4fb3-b363-3b7fe8e74483";

    public const string KeyVaultContributor = "f25e0fa2-a7c8-4377-a976-54943a77a395";

    public const string KeyVaultCertificatesOfficer = "a4417e6f-fecd-4de8-b567-7b0420556985";

    public const string KeyVaultSecretsUser = "4633458b-17de-408a-b874-0445c86b69e6";

    public const string KeyVaultSecretsOfficer = "b86a8fe4-44ce-4948-aee5-eccb2c155cd7";

    public const string KeyVaultReader = "21090545-7ca7-4776-b22c-e363652d74d2";


    #endregion

    #region Storage

    public const string StorageAccountContributor = "17d1049b-9a84-46fb-8f53-869881c3d3ab";

    public const string ReaderAndDataAccess = "c12c1c16-33a1-487b-954d-41c89c60f349";

    public const string StorageBlobDataOwner = "b7e6dc6d-f1e8-4753-8033-0f276bb0955b";

    public const string StorageBlobDataReader = "2a2b9908-6ea1-4ae2-8e65-a410df84e7d1";

    public const string StorageBlobDataContributor = "ba92f5b4-2d11-453d-a403-e96b0029c9fe";

    public const string StorageTableDataContributor = "0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3";

    public const string StorageTableDataReader = "76199698-9eea-4c19-bc75-cec21354c6b6";

    #endregion

    #region Databases

    public const string DocumentDbAccountContributor = "5bd9cd88-fe45-4216-938b-f97437e15450";

    public const string CosmosDbAccountReaderRole = "fbdf93bf-df7d-467e-a4d2-9458aa1360c8";

    public const string CosmosDbOperator = "230815da-be43-4aae-9cb4-875f7bd000aa";

    public const string CosmosDbBackupOperator = "db7b14f2-5adf-42da-9f96-f2ee17bab5cb";

    public const string CosmosDbRestoreOperator = "5432c526-bc82-444a-b7ba-57c5b0b5b34f";

    public const string SqlDbContributor = "9b7fa17d-e63e-47b0-bb0a-15c516ac86ec";

    public const string SqlServerContributor = "6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437";

    #endregion

    #region Containers

    public const string AcrPull = "7f951dda-4ed3-4680-a7ca-43fe172d538d";

    public const string AcrDelete = "c2f4ef07-c644-48eb-af81-4b1b4947fb11";

    public const string AcrPush = "8311e382-0749-4cb8-b61a-304f252e45ec";

    public const string AcrImageSigner = "6cef56e8-d556-48e5-a04f-b8e64114680f";

    public const string AcrQuarantineReader = "cdda3590-29a3-44f6-95f2-9f980659eb04";

    public const string AcrQuarantineWriter = "c8d4ff99-41c3-41a8-9f60-21dfdad59608";

    public const string AzureKubernetesServiceClusterAdminRole = "0ab0b1a8-8aac-4efd-b8c2-3ee1fb270be8";

    public const string AzureKubernetesServiceClusterUserRole = "4abbcc35-e782-43d8-92c5-2d3f1bd2253f";

    public const string AzureKubernetesServiceContributorRole = "ed7f3fbd-7b88-4dd4-9017-9adb7ce333f8";

    public const string AzureKubernetesServiceRbacClusterAdmin = "b1ff04bb-8a4e-4dc4-8eb5-8693973ce19b";

    public const string AzureKubernetesServiceRbacAdmin = "3498e952-d568-435e-9b2c-8d77e338d7f7";

    public const string AzureKubernetesServiceRbacReader = "7f6c6a51-bcf8-42ba-9220-52d62157d7db";

    public const string AzureKubernetesServiceRbacWriter = "a7ffa36f-339b-4b5c-8bdf-e2c188b2c0eb";

    #endregion

    #region Integration

    public const string ApiManagementServiceContributor = "312a565d-c81f-4fd8-895a-4e21e48d571c";
    public const string ApiManagementServiceOperatorRole = "e022efe7-f5ba-4159-bbe4-b44f577e9b61";
    public const string ApiManagementServiceReaderRole = "71522526-b88f-4d52-b57f-d31fc3546d0d";

    public const string AzureServiceBusDataOwner = "090c5cfd-751d-490a-894a-3ce6f1109419";
    public const string AzureServiceBusDataReceiver = "4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0";
    public const string AzureServiceBusDataSender = "69a216fc-b8fb-44d8-bc22-1f3c2cd27a39";

    #endregion
}