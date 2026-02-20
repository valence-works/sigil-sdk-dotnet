// Spec 004: Typed claims model for license:v1 statement.

namespace Sigil.Sdk.Validation;

public sealed class LicenseClaims
{
	public LicenseClaims(
		string productId,
		string edition,
		IReadOnlyList<string> features,
		long expiresAt,
		int maxSeats,
		long? issuedAt = null)
	{
		if (string.IsNullOrWhiteSpace(productId))
		{
			throw new ArgumentException("ProductId cannot be null or whitespace.", nameof(productId));
		}

		if (string.IsNullOrWhiteSpace(edition))
		{
			throw new ArgumentException("Edition cannot be null or whitespace.", nameof(edition));
		}

		if (features is null)
		{
			throw new ArgumentNullException(nameof(features));
		}

		if (maxSeats <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxSeats), "MaxSeats must be greater than zero.");
		}

		ProductId = productId;
		Edition = edition;
		Features = features;
		ExpiresAt = expiresAt;
		MaxSeats = maxSeats;
		IssuedAt = issuedAt;
	}

	public string ProductId { get; }

	public string Edition { get; }

	public IReadOnlyList<string> Features { get; }

	public long ExpiresAt { get; }

	public int MaxSeats { get; }

	public long? IssuedAt { get; }
}
